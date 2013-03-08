import smtplib
import string
import subprocess
import datetime
import sys
from collections import namedtuple
import shutil
import re
import os

from email.MIMEMultipart import MIMEMultipart
from email.MIMEBase import MIMEBase
from email.MIMEText import MIMEText
from email import Encoders

def main():

	# CONSTANTS

	# git 
	remote_path = "https://github.com/ikeough/Dynamo.git"

	# email and log
	email_sender = "peter.boyer@autodesk.com"   				
	email_list_path = "emails.txt"

	# source 
	repo_date = date_string()
	sandbox_path = form_path(['repos',repo_date])

	repo_name = "Dynamo"
	repo_root = form_path( [sandbox_path, repo_name ] )
	solution_path = form_path( [repo_root, '/Dynamo.sln'] )

	# installer
	installer_dir =  form_path( [repo_root, 'DynamoWIPInstall'] )
	installer_bin_dir = 'Installers'
	installer_bat = 'CreateInstallers-RELEASE.bat'

	#realtimedev
	realtimedev_root = 'C:/Users/boyerp/Dropbox/DynamoRealtimeDev'  

	# build
	msbuild_path = "C:/Windows/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe"

	#log
	log_prefix = form_path([sandbox_path, "dynamo_auto_build_log_"])
	
	# do auto-build!
	setup(sandbox_path)

	print 'cloning...'
	pull_result = clone( remote_path, sandbox_path, repo_name ) 

	print 'getting commit history...'
	commits = run_cmd([ 'git','log','--since=1.day'], cwd = repo_root)
	print commits
	
	print 'building....'
	build_result = build( msbuild_path, solution_path )
	build_result_debug = build( msbuild_path, solution_path, build_config = "Debug" )

	installers_result = "Installers not formed.\n"

	if (build_result['success'] == True and build_result_debug['success'] == True):
		print 'making installers...'
		installers_result = make_installers( installer_dir, installer_bat )

		print 'copying installers and binaries to realtime-dev...'
		update_realtimedev( installer_dir, installer_bin_dir, repo_root, realtimedev_root )
		
	else:
		print 'build failed'

	print 'interpreting results...'
	[subject, message] = get_email_content( repo_date, pull_result, [build_result, build_result_debug], installers_result, commits )
	
	print 'logging...'
	log = log_results(log_prefix, pull_result, [build_result, build_result_debug], installers_result, commits)

	print 'emailing results...'
	email_result = email_all( email_list_path, email_sender, subject, message, log )


def interpret_build(result):

	errors = int( re.search( r"[0-9]* Error", result).group(0).split(' ')[0] )
	warnings = int( re.search( r"[0-9]* Warning", result).group(0).split(' ')[0] )

	return {'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }

def setup(path):
	mkdir(path)

def cleanup(pull_to):
	rm_dir(pull_to)

def rm_dir(path):
	if os.path.exists(path):
		run_cmd(['rmdir', path, '/S', '/Q'])

def mkdir(path):
	if os.path.exists(path):
		return
	os.makedirs(path)

def update_realtimedev( installer_dir, installer_bin_dir, repo_root, realtimedev_root ):

	if not os.path.exists(realtimedev_root):
		print 'realtimedev_root does not exist: ' + realtimedev_root
		return None

	mkdir( form_path( [realtimedev_root, "builds"] ) )
	release_dir = form_path( [realtimedev_root, "builds", date_string()] )
	install_dir = form_path( [release_dir, "install"] ) 
	bin_dir = form_path( [release_dir, "bin"] )
	mkdir( install_dir )
	mkdir( bin_dir )

	# copy contents of installer_bin_dir, contents of installer_dir
	copy_folder_contents( form_path([installer_dir, installer_bin_dir]), install_dir )
	copy_folder_contents( form_path([repo_root, 'bin']), bin_dir )

def copy_folder_contents(path, endpath):
	return run_cmd(['robocopy', path, endpath, '/E' ])

def make_installers(path_to_installer_bats, installer_bat):
	return run_cmd( installer_bat, cwd = path_to_installer_bats )

def log_results(log_prefix, pull_result, build_result, email_result, commits):
	path = log_prefix + date_string() + '.log'
	
	log = [pull_result, commits, build_result[0]["result"], build_result[1]["result"], email_result]
	log = map( lambda v: str(v), log )  
	log_string = '\n############\n'.join( log )

	with open(path, 'w+') as f:
		f.write( log_string )

	return log_string
	
def date_string():
	return datetime.datetime.now().strftime("%Y%m%dT%H%M%S")
	
def clone( git_repo_path_https, pull_dir, repo_name ):

	return run_cmd(['git','clone', git_repo_path_https], cwd = pull_dir )		
	
def form_path(elements):
	return "/".join( elements )

def build( compiler_path, solution_path, build_config = "Release" ):
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
	
def get_email_content( repo_date, pull_result, build_results, installer_result, commits ):

	if (installer_result == None):
		installer_result = "No installers created"

	success_string = "Failure"
	success = False

	if (build_results[0]["success"] == True and build_results[1]["success"] == True):
		success = True
		success_string = "Success"

	subject = "Dynamo autobuild for " + repo_date + ": " + success_string
	message = "The build was a " + success_string + "\n\n"
	message = message + "------------------------- \n\n"
	message = message + "There were " + str( build_results[0]["errors"]) + " errors and "
	message = message + str( build_results[0]["warnings"] ) + " warnings in the Release build. \n\n"

	message = message + "There were " + str( build_results[1]["errors"]) + " errors and "
	message = message + str( build_results[0]["warnings"] ) + " warnings in the Debug build. \n\n"
	message = message + "------------------------- \n\n"
	message = message + "Commits since last build: \n\n"
	message = message + commits
	message = message + "------------------------- \n\n"

	if success:
		message = message + "The installers were copied to the DynamoRealtimeDev/builds/" + repo_date + "/install\n\n"

	message = message + "Attached is a log.\n\n"

	message = message + "Peace out, silicon representin\n\n"

	return [subject, message]
	
def get_recipients( path ):

	with open(path) as f:
		content = f.readlines()
		return map( lambda line: line.rstrip(), content )
	return []

def email_all( emails_path, sender, subject, message, in_log ):
	
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