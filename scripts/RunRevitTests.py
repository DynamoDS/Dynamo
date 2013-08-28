import sys
import os
import fnmatch
import subprocess
import xml.etree.ElementTree as ET
import argparse

def main():

	#http://stackoverflow.com/questions/7427101/dead-simple-argparse-example-wanted-1-argument-3-results
	parser = argparse.ArgumentParser(description='Run Revit Tests')
	parser.add_argument('-n','--name', help='Name of a test to run.', required=False)
	args = vars(parser.parse_args())
	# print args

	# find all the journal files
	# journal files will end in .txt
	# these should be the only .txt files in the directory
	testsPath = os.path.join(os.path.dirname(__file__), "..\\test\\revit")
	#print(testsPath)

	testDyns = [os.path.join(dirpath, f)
		for dirpath, dirnames, files in os.walk(testsPath)
		for f in fnmatch.filter(files, '*.dyn')]

	testJournals = [os.path.join(dirpath, f)
    	for dirpath, dirnames, files in os.walk(testsPath)
    	for f in fnmatch.filter(files, '*.txt')]

	print '{0} dyn files with {1} tests'.format(len(testDyns), len(testJournals))

	# run a test by name
	if args['name'] is not None:
		# find the journal with the name
		# like the name argument
		# i.e. if the name is CurveLoop, look for CurveLoop.txt
		for journal in testJournals:
			base = os.path.basename(journal)
			if base == args['name']+'.txt':
				# print os.path.basename(journal)
				run_cmd(['Revit', os.path.abspath(journal)])
	# run all tests
	else:
		print 'running revit tests....'
		for journal in testJournals:
			print 'running ' + journal
			run_cmd( ['Revit', os.path.abspath(journal)] )

	# aggregate results
	results = [os.path.join(dirpath, f)
    	for dirpath, dirnames, files in os.walk(testsPath)
    	for f in fnmatch.filter(files, '*_result.xml')]

	for result in results:
		tree = ET.parse(result)
		root = tree.getroot()
		for member in root.iter('DynamoRevitTest'):
			testName = member.get('TestName')
			result_type = member.find('ResultType').text
			message_element = member.find('Message')
			message = ''
			if message_element is not None:
				message = message_element.text
			print testName + ':' + result_type
			if message is not '':
				print '\t' + message

	# cleanup results files and journals created during testing
	# print 'cleaning journal files...'
	# runJournals = [os.path.join(dirpath, f)
 #    	for dirpath, dirnames, files in os.walk(testsPath)
 #    	for f in fnmatch.filter(files, 'journal.*')]

	# for journal in runJournals:
	# 	os.remove(journal)

	# print 'cleaning results files...'
	# results = [os.path.join(dirpath, f)
 #    	for dirpath, dirnames, files in os.walk(testsPath)
 #    	for f in fnmatch.filter(files, '*_result.xml')]

	# for result in results:
	# 	os.remove(result)

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