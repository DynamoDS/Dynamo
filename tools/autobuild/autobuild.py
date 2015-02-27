import smtplib
import string
import subprocess
import datetime
import sys
import shutil
import re
import os
import fnmatch
import dynamo_s3
from optparse import OptionParser

from email.MIMEMultipart import MIMEMultipart
from email.MIMEBase import MIMEBase
from email.MIMEText import MIMEText
from email import Encoders

def main():

	parser = OptionParser()
	
	parser.add_option("-r", "--remote", dest="remote", help="The URI of the git remote.", metavar="FILE", default="https://github.com/ikeough/Dynamo.git")
	parser.add_option("-f", "--force", action="store_true", dest="force_build_without_commits", default=False, help="Run build even if no commits have taken place")
	parser.add_option("-s", "--sender", dest="email_sender", default="peter.boyer@autodesk.com" , help="Who the email will be sent from")
	parser.add_option("-e", "--emails", dest="email_list_path", metavar="FILE", default="emails.txt" , help="A \n delimited list of emails to send the results to")
	parser.add_option("-d", "--date", dest="repo_date", default=date_string(), help="Override the date of the build")
	parser.add_option("-n", "--repo_name", dest="repo_name", default="Dynamo" , help="The name of the repo.  Don't screw this up.")
	parser.add_option("-z", "--debug", dest="debug", action="store_true", default=False, help="Run debug - don't send emails or update realtimedev.")
	parser.add_option("-p", "--sol_path", dest="solution_path", default="src/Dynamo.sln", help="Solution path relative to repo root.")
	parser.add_option("-o", "--autodoc_root", dest="autodoc_root", default="scripts/autodoc", help="Autodoc root path relative to repo root.")
	parser.add_option("-i", "--autodoc_name", dest="autodoc_name", default="autodoc.py", help="Name of autodoc script")
	parser.add_option("-v", "--realtimedev", dest="realtimedev_root", default="C:/Users/boyerp/Dropbox/DynamoRealtimeDev", help="Root of realtime dev directory")
	parser.add_option("-b", "--msbuild_path", dest="msbuild_path", default="C:/Windows/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe", help="Path to MSBuild.exe")
	parser.add_option("-c", "--commit_period", dest="commit_period", default="day", help="Amount of time to log commits")
	(options, args) = parser.parse_args()

	sandbox_path = form_path(['repos',options.repo_date])
	repo_root = form_path( [sandbox_path, options.repo_name ] ) 
	solution_path = form_path( [repo_root, options.solution_path] ) 
	autodoc_root = form_path( [repo_root, options.autodoc_root] )
	options.autodoc_name = 'autodoc.py'
	installer_dir =  form_path( [repo_root, 'scripts/install'] )
	installer_bin_dir = 'Installers'
	installer_bat = 'CreateInstallers-RELEASE.bat'
	log_prefix = form_path([sandbox_path, "dynamo_auto_build_log_"])
	
	# do auto-build!
	setup(sandbox_path)

	pull_result = clone( options.remote, sandbox_path, options.repo_name ) 

	print 'getting commit history...'
	commits = run_cmd([ 'git','log',"--since=1."+options.commit_period], cwd = repo_root)

	if commits == "" and not options.force_build_without_commits:
		message = "No build run as there were no commits for the last " + options.commit_period
		print message
		if not options.debug:
			email_result = email_all( options.email_list_path, options.email_sender, "Dynamo autobuild skipped for " + options.repo_date, message, "" )
		return

	build_result = build( options.msbuild_path, solution_path, build_config = "Release" )
	build_result_debug = build( options.msbuild_path, solution_path, build_config = "Debug" )

	unit_test_result = {}
	if build_result["success"] == True and build_result_debug["success"] == True:
		unit_test_result = interpret_unit_tests( run_unit_tests( repo_root ) )

	installers_result = "Installers not formed.\n"

	if (build_result['success'] == True and build_result_debug['success'] == True):

		autodoc_result = make_docs( autodoc_root, options.autodoc_name )

		installers_result = make_installers( installer_dir, installer_bat )

		if not options.debug:
			update_realtimedev( installer_dir, installer_bin_dir, repo_root, autodoc_root, options.realtimedev_root )
			publish_to_s3( installer_dir, installer_bin_dir )

	else:
		print 'build failed'

	[subject, message] = get_email_content( options.repo_date, pull_result, [build_result, build_result_debug], installers_result, commits, unit_test_result )
	
	log = log_results(log_prefix, pull_result, [build_result, build_result_debug], installers_result, commits, unit_test_result )

	if not options.debug:
		email_result = email_all( options.email_list_path, options.email_sender, subject, message, log )


def interpret_build(result):

	errors = int( re.search( r"[0-9]+ Error", result).group(0).split(' ')[0] )
	warnings = int( re.search( r"[0-9]+ Warning", result).group(0).split(' ')[0] )

	return {'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }

def publish_to_s3(installer_dir, installer_bin_dir):

	try:

		mkdir('temp')
		copy_folder_contents( form_path([installer_dir, installer_bin_dir]), 'temp' )

		for file in os.listdir('temp'):
		  if fnmatch.fnmatch( file, '*.exe'):
		  	dynamo_s3.upload_daily( form_path( ['temp', file] ) )

	except Exception:
		print 'There was an exception uploading to s3.'
	finally:
		rm_dir('temp')
	
def setup(path):
	mkdir(path)

def cleanup(pull_to):
	rm_dir(pull_to)

def rm_dir(path):
	if os.path.exists(path):
		run_cmd(['rmdir', path, '/S', '/Q'])

def interpret_unit_tests( result ):

	if (result == None or result == ""):
		result = "Unit tests failed to run"

	parsed_results = {}
	
	try: 
	
		parsed_results['tests_run'] = int( re.search( r"Tests run: [0-9]+", result).group(0).split(' ')[2] )
		parsed_results['errors'] = int( re.search( r"Errors: [0-9]+", result).group(0).split(' ')[1] )
		parsed_results['failures'] = int( re.search( r"Failures: [0-9]+", result).group(0).split(' ')[1] )
		parsed_results['inconclusives'] = int( re.search( r"Inconclusive: [0-9]+", result).group(0).split(' ')[1] )
		parsed_results['time'] = float( re.search( r"Time: [0-9.]+", result).group(0).split(' ')[1] )

		parsed_results['not_run'] = int( re.search( r"Not run: [0-9]+", result).group(0).split(' ')[2] )
		parsed_results['invalid'] = int( re.search( r"Invalid: [0-9]+", result).group(0).split(' ')[1] )
		parsed_results['ignored'] = int( re.search( r"Ignored: [0-9]+", result).group(0).split(' ')[1] )
		parsed_results['skipped'] = int( re.search( r"Skipped: [0-9]+", result).group(0).split(' ')[1] )

		parsed_results["success"] = False

		if parsed_results['errors'] == 0 and parsed_results['failures'] == 0 and parsed_results['inconclusives'] == 0:
			parsed_results["success"] = True

		parsed_results["errors_and_failures"] = ""

		parsed_results["full_results"] = result

		ef = re.search(r"\nErrors and Failures:(.|\n)+\r\n\r\n\n", result)

		if ef != None:
			parsed_results["errors_and_failures"] = ef.group(0)

		return parsed_results

	except: 
		
		return parsed_results

def enumerate_tests(path):

	# match *Tests.dll
	testnames = []
	for file in os.listdir(path):
	    if fnmatch.fnmatch(file, '*Tests.dll'):
	        testnames.append(file)
	return testnames
	
def run_unit_tests(path, build_config = "Debug"):

	print 'running unit tests....'
	return run_cmd( ['nunit-console', '/noshadow'] + enumerate_tests(form_path( [path, "bin", build_config ])), cwd= form_path( [path, "bin", build_config ]) )

def mkdir(path):

	if os.path.exists(path):
		return

	os.makedirs( path )

def make_docs( autodoc_root, autodoc_name ):

	print 'making docs...'
	return run_cmd(['python',autodoc_name], cwd = autodoc_root )	

def update_realtimedev( installer_dir, installer_bin_dir, repo_root, autodoc_root, realtimedev_root ):

	print 'copying installers, docs, and binaries to realtime-dev...'
	if not os.path.exists(realtimedev_root):
		print 'realtimedev_root does not exist: ' + realtimedev_root
		return None

	mkdir( form_path( [realtimedev_root, "builds"] ) )
	release_dir = form_path( [realtimedev_root, "builds", date_string()] )
	install_dir = form_path( [release_dir, "install"] ) 
	bin_dir = form_path( [release_dir, "bin"] )
	doc_dir = form_path( [release_dir, "doc"] )
	mkdir( install_dir )
	mkdir( bin_dir )
	mkdir( doc_dir )

	copy_folder_contents( form_path([installer_dir, installer_bin_dir]), install_dir )
	copy_folder_contents( form_path([repo_root, 'bin']), bin_dir )
	copy_folder_contents( form_path([autodoc_root, 'out']), doc_dir )

def copy_folder_contents(path, endpath):
	return run_cmd(['robocopy', path, endpath, '/E' ])

def make_installers(path_to_installer_bats, installer_bat):
	print 'making installers...'
	return run_cmd( installer_bat, cwd = path_to_installer_bats )

def log_results(log_prefix, pull_result, build_result, email_result, commits, unit_test_result):
	print 'logging...'
	path = log_prefix + date_string() + '.log'
	
	log = [pull_result, commits, build_result[0]["result"], build_result[1]["result"], unit_test_result, email_result]
	log = map( lambda v: str(v), log )  
	log_string = '\n############\n'.join( log )

	with open(path, 'w+') as f:
		f.write( log_string )

	return log_string
	
def date_string():
	return datetime.datetime.now().strftime("%Y%m%dT%H%M%S")
	
def clone( git_repo_path_https, pull_dir, repo_name ):
	print 'cloning...'
	return run_cmd(['git','clone', git_repo_path_https], cwd = pull_dir )		
	
def form_path(elements):
	return "/".join( elements )

def build( compiler_path, solution_path, build_config = "Release" ):

	print 'building....'
	return interpret_build( run_cmd([compiler_path, '/p:Configuration='+build_config, solution_path]) )
		
def run_cmd( args, printOutput = True, cwd = None ):	
	p = subprocess.Popen(args, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd = cwd)
	
	out = ''

	while True:
	    line = p.stdout.readline()
	   
	    if not line:
	        break
	    out = out + line
	    print ">>> " + line.rstrip()
		
	return out
	
def get_email_content( repo_date, pull_result, build_results, installer_result, commits, unit_test_result):

	print 'interpreting results...'

	if (installer_result == None):
		installer_result = "No installers created"

	success_string = "FAILURE"
	success = False

	if (build_results[0]["success"] == True and build_results[1]["success"] == True):
		success = True
		success_string = "Success"

	test_success_string = "FAILURE"
	tests_run = False

	print unit_test_result

	if "success" in unit_test_result and unit_test_result["success"] == True:
		test_success_string = "Success"
	
	if "success" in unit_test_result:
		tests_run = True

	# Header
	subject = "Dynamo autobuild - Build: " + success_string + ", Tests: " + test_success_string + ", Date: " + repo_date 
	message = "The build was a " + success_string + "\n\n"

	# Installers
	if success:
		message = message + "The installers were copied to the DynamoRealtimeDev/builds/" + repo_date + "/install\n\n"
		message = message + "The node docs were copied to the DynamoRealtimeDev/builds/" + repo_date + "/dev\n\n"

	message = message + "Attached is a log.\n\n"

	#Errors, warnings, etc
	message = message + "------------------------- \n\n"
	message = message + "There were " + str( build_results[0]["errors"]) + " errors and "
	message = message + str( build_results[0]["warnings"] ) + " warnings in the Release build. \n\n"

	message = message + "There were " + str( build_results[1]["errors"]) + " errors and "
	message = message + str( build_results[0]["warnings"] ) + " warnings in the Debug build. \n\n"
	message = message + "------------------------- \n\n"

	message = message + "Dynamo Elements Unit Tests: \n\n"

	if tests_run:

		# Unit tests
		if "success" in unit_test_result and unit_test_result["success"] == True:
			message = message + "Success"
		else:
			message = message + "FAILURE."

		message = message + "\n\n" 
		message = message + "Tests run: " + str( unit_test_result["tests_run"] )
		message = message + "  Errors: " + str( unit_test_result["errors"] )
		message = message + "  Failures: " + str( unit_test_result["failures"] )
		message = message + "  Time elapsed: " + str( unit_test_result["time"] ) + "\n\n"
		message = message + "\n\n" + "Full results: " + "\n\n" + str( unit_test_result["full_results"] )
		message = message + "\n\n------------------------- \n\n"

	else:
		message = message + "Unit tests were not run.  This is likely because of a build failure."
		message = message + "\n\n------------------------- \n\n"

	# Copies
	message = message + "Commits since last build: \n\n"
	message = message + commits
	message = message + "\n\n------------------------- \n\n"

	return [subject, message]
	
def get_recipients( path ):

	with open(path) as f:
		content = f.readlines()
		return map( lambda line: line.rstrip(), content )
	return []

def email_all( emails_path, sender, subject, message, in_log ):
	
	print 'emailing results...'

	log = ""
	
	recipients = get_recipients( emails_path )

	for recip in recipients:
		send_email_with_attachment(subject, message, in_log, sender, recip)
			
	return log
		
def send_email( subject, text, receiver, sender ):

	message = string.join((
				"From: %s" % sender,
				"To: %s" % receiver,
				"Subject: %s" % subject ,
				"",
				text
				), "\r\n")

	try:
	   smtpObj = smtplib.SMTP('mail-relay.autodesk.com')
	   smtpObj.sendmail(sender, receiver, message)         
	   print ("Successfully sent email to " + receiver)
	except SMTPException:
	   print ("Error: unable to send email to " + receiver)

def send_email_with_attachment(subject, text, log, sender, receiver):

	msg = MIMEMultipart()
	msg['From']= sender
	msg['To'] = receiver
	msg['Subject'] = subject
	
	msg.attach( MIMEText(text) )

	part = MIMEBase('application', "octet-stream")
	part.set_payload(log)
	Encoders.encode_base64(part)

	part.add_header('Content-Disposition', 'attachment; filename="build_log.txt"')
	msg.attach(part)

	try:
	   smtpObj = smtplib.SMTP('mail-relay.autodesk.com')
	   smtpObj.sendmail(sender, receiver, msg.as_string())         
	   print ("Successfully sent email to " + receiver)
	except SMTPException:
	   print ("Error: unable to send email to " + receiver)

if __name__ == "__main__":
    main()