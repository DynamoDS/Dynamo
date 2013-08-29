import smtplib
import string
import subprocess
import datetime
import sys
import shutil
import re
import os
import fnmatch
from optparse import OptionParser


def main():

	parser = OptionParser()
	
	parser.add_option("-r", "--root", dest="root", default=os.getcwd() + "\\..\\..", help="The root path of the repo")
	parser.add_option("-n", "--repo_name", dest="repo_name", default="Dynamo" , help="The name of the repo.  Don't screw this up.")
	parser.add_option("-p", "--sol_path", dest="solution_path", default="src/Dynamo.sln", help="Solution path relative to repo root.")
	parser.add_option("-t", "--no_test", action="store_false", dest="test", default=True, help="Don't run tests")
	parser.add_option("-b", "--msbuild_path", dest="msbuild_path", default="C:/Windows/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe", help="Path to MSBuild.exe")
	
	(options, args) = parser.parse_args()

	print os.getcwd()

	# form path
	repo_root = options.root
	solution_path = form_path( [repo_root, options.solution_path] ) 
	
	# perform build
	build_result = build( options.msbuild_path, solution_path, build_config = "Release" )
	build_result_debug = build( options.msbuild_path, solution_path, build_config = "Debug" )

	# perform unit tests
	unit_test_result = {}
	if build_result["success"] == True and build_result_debug["success"] == True and options.test:
		unit_test_result = interpret_unit_tests( run_unit_tests( repo_root ) )

	# print results
	result = get_email_content([build_result, build_result_debug], unit_test_result )
	for line in result[1].split("\n"):
		print line

def interpret_build(result):

	errors = int( re.search( r"[0-9]+ Error", result).group(0).split(' ')[0] )
	warnings = int( re.search( r"[0-9]+ Warning", result).group(0).split(' ')[0] )

	return {'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }
	
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

def copy_folder_contents(path, endpath):
	return run_cmd(['robocopy', path, endpath, '/E' ])

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
	
def get_email_content(  build_results, unit_test_result):

	print '\n\n\nBUILD RESULTS\n\n'

	success_string = "FAILURE"
	success = False

	if (build_results[0]["success"] == True and build_results[1]["success"] == True):
		success = True
		success_string = "Success"

	test_success_string = "FAILURE"
	tests_run = False

	if "success" in unit_test_result and unit_test_result["success"] == True:
		test_success_string = "Success"
	
	if "success" in unit_test_result:
		tests_run = True

	# Header
	subject = "Dynamo autobuild - Build: " + success_string + ", Tests: " + test_success_string
	message = "The build was a " + success_string + "\n\n"

	#Errors, warnings, etc
	message = message + "------------------------- \n\n"
	message = message + "There were " + str( build_results[0]["errors"]) + " errors and "
	message = message + str( build_results[0]["warnings"] ) + " warnings in the Release build. \n\n"

	message = message + "There were " + str( build_results[1]["errors"]) + " errors and "
	message = message + str( build_results[0]["warnings"] ) + " warnings in the Debug build. \n\n"
	message = message + "------------------------- \n\n\n"

	message = message + "TEST RESULTS \n\n\n"

	
	if tests_run:

		message = message + "Tests were a "

		# Unit tests
		if "success" in unit_test_result and unit_test_result["success"] == True:
			message = message + "Success"
		else:
			message = message + "FAILURE"

		message = message + "\n\n------------------------- \n\n"
		message = message + "Tests run: " + str( unit_test_result["tests_run"] )
		message = message + "  Errors: " + str( unit_test_result["errors"] )
		message = message + "  Failures: " + str( unit_test_result["failures"] )
		message = message + "  Time elapsed: " + str( unit_test_result["time"] ) + "\n\n"
		# message = message + "\n\n" + "Full results: " + "\n\n" + str( unit_test_result["full_results"] )

	else:
		message = message + "------------------------- \n\n"
		message = message + "Unit tests were not run.  \n\n"
	
	message = message + "------------------------- \n\n"


	return [subject, message]
	
if __name__ == "__main__":
    main()