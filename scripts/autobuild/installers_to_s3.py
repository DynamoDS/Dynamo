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

def main():

	parser = OptionParser()
	parser.add_option("-r", "--root", dest="root", help="The root directory of the dynamo project.", metavar="FILE", default="E:/Dynamo")

	(options, args) = parser.parse_args()

	repo_root = options.root
	installer_dir =  form_path( [repo_root, 'scripts/install'] )
	installer_bin_dir = 'Installers'
	installer_bat = 'CreateInstallers-RELEASE.bat'
	print "Publishing to s3"
	print installer_dir
	print installer_bin_dir
	
	publish_to_s3( installer_dir, installer_bin_dir )

# S3 ##############################

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
	p = subprocess.Popen(args, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd = cwd)
	
	out = ''

	while True:
	    line = p.stdout.readline()
	   
	    if not line:
	        break
	    out = out + line
	    print ">>> " + line.rstrip()
		
	return out

if __name__ == "__main__":
    main()