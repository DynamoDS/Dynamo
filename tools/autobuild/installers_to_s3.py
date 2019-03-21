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
import traceback

def main():

	parser = OptionParser()
	parser.add_option("-r", "--root", dest="root", help="The root directory of the dynamo project.", metavar="FILE", default="E:/Dynamo")
	parser.add_option("-p", "--prefix", dest="prefix", help="The prefix on the filename for the object.", metavar="FILE", default="DynamoDailyInstall")
	parser.add_option("-d", "--nodate", dest="include_date", action="store_false", default=True, help="Add the date as a suffix to the filename.")
	parser.add_option("-v", "--dev", dest="dev_build", action="store_true", default=False, help="This is a development build.")

	(options, args) = parser.parse_args()

	installer_dir = options.root

	print('Publishing to s3')
	print(installer_dir)
	
	publish_to_s3( installer_dir, options.prefix, options.include_date, options.dev_build )

# S3 ##############################

def publish_to_s3(installer_dir, prefix, include_date, is_dev_build):

	try:

		mkdir('temp')
		copy_folder_contents( installer_dir, 'temp')

		date_string = dynamo_s3.date_string()

		for file in os.listdir('temp'):
		  if fnmatch.fnmatch( file, '*.exe'):
		  	dynamo_s3.upload_installer( form_path( ['temp', file] ), prefix, include_date, is_dev_build ,'.exe', date_string)
		  elif fnmatch.fnmatch( file, '*.sig'):
		  	dynamo_s3.upload_installer( form_path( ['temp', file] ), prefix, include_date, is_dev_build ,'.sig', date_string)
		  elif fnmatch.fnmatch( file, '*.zip'):
		  	dynamo_s3.upload_installer( form_path( ['temp', file] ), prefix, include_date, is_dev_build ,'.zip', date_string)

	except Exception:
		print('There was an exception uploading to s3:')
		print(traceback.format_exc())
	finally:
		rm_dir('temp')
	
# UTILITIES ##############################

def rm_dir(path):
	if os.path.exists(path):
		run_cmd(['rmdir', path, '/S', '/Q'])

def mkdir(path):

	if os.path.exists(path):
		return

	os.makedirs( path )

def copy_folder_contents(path, endpath):
	return run_cmd(['robocopy', path, endpath, '/E' ])

def date_string():
	return datetime.datetime.now().strftime("%Y%m%dT%H%M%S")

def form_path(elements):
	return "/".join( elements )

def run_cmd( args, printOutput = True, cwd = None ):	
	p = subprocess.Popen(args, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd = cwd, universal_newlines=True)
	
	out = ''

	while True:
	    line = p.stdout.readline()
	   
	    if not line:
	        break
	    out = out + line
	    print(">>> " + line.rstrip())
		
	return out

if __name__ == "__main__":
    main()