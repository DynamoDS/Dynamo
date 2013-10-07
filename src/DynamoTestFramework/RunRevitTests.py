import sys
import os
import fnmatch
import subprocess
import xml.etree.ElementTree as ET
import argparse
import time
import glob
import smtplib
import string

def main():

	#http://stackoverflow.com/questions/7427101/dead-simple-argparse-example-wanted-1-argument-3-results
	parser = argparse.ArgumentParser(description='Run Revit Tests')
	parser.add_argument('-n','--name', help='Name of a test to run.', required=True)
	parser.add_argument('-f','--fixture', help='Name of fixture to run.', required=False)
	parser.add_argument('-a','--assembly', help='Path of the assembly containing tests.', required=True)
	parser.add_argument('-o','--output', help='Output location of the results file.', required=True)
	parser.add_argument('-m','--model', help='Path of the model file for open.', required=True)
	args = vars(parser.parse_args())
	# print args

	start_time = time.time()

	#Generate a temporary journal file
	journal = generate_journal_file(args['name'], args['fixture'], args['assembly'], args['output'], args['model'])

	#Run Revit passing the journal file as a parameter
	print 'running ' + journal
	run_cmd( ['Revit', os.path.abspath(journal)] )

	#Cleanup temporary journal file
 	os.remove(journal)

	sys.exit(0)

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

def generate_journal_file(testName, fixtureName, testAssembly, resultsPath, modelName):
	
	currPath = os.getcwd()

	# change into the template path and 
	# read the template file
	os.chdir(os.path.abspath("./template"))
	template = glob.glob("DynamoTemplate.txt")[0]
	s =''
	with open(template, 'r') as templateFile:
		s = string.Template(templateFile.read())
	templateFile.closed

	# change back to the test directory
	os.chdir(os.path.abspath(currPath))

	content = s.substitute(testName=testName, fixtureName=fixtureName, testAssembly=testAssembly, resultsPath=resultsPath, modelName=modelName)

	# save the content to a file
	journal = "DynamoTestFrameworkTmp.txt"
	with open(journal, 'w') as f:
		f.write(content)
	f.closed

	return journal

if __name__ == "__main__":
	main()