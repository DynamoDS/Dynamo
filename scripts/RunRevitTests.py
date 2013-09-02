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
	parser.add_argument('-n','--name', help='Name of a test to run.', required=False)
	args = vars(parser.parse_args())
	# print args

	start_time = time.time()

	# find all the journal files
	# journal files will end in .txt
	# these should be the only .txt files in the directory
	testsPath = os.path.join(os.path.dirname(__file__), "..\\test\\revit")
	#print(testsPath)

	# search for .dyn and .txt files NON-RECURSIVELY
	os.chdir(testsPath)
	testDyns = glob.glob("*.dyn")
	testJournals = glob.glob("*.txt")

	# testDyns = [os.path.join(dirpath, f)
	# 	for dirpath, dirnames, files in os.walk(testsPath)
	# 	for f in fnmatch.filter(files, '*.dyn')]

	# testJournals = [os.path.join(dirpath, f)
 #    	for dirpath, dirnames, files in os.walk(testsPath)
 #    	for f in fnmatch.filter(files, '*.txt')]

	print '{0} dyn files with {1} journals'.format(len(testDyns), len(testJournals))

	# run a test by name
	if args['name'] is not None:
		# find the journal with the name
		# like the name argument
		# i.e. if the name is CurveLoop, look for CurveLoop.txt
		found = False
		for journal in testJournals:
			base = os.path.basename(journal)
			if base == args['name']+'.txt':
				# print os.path.basename(journal)
				run_cmd(['C:\Program Files\Autodesk\Revit Architecture 2014\Revit.exe', os.path.abspath(journal)])
				found = True
		if found == False:
			print "Journal named {0} could not be found".format(args['name'])
			return

	# run all tests
	else:
		print 'running revit tests....'
		
		# EXPERIMENTAL* mode for running tests in parallel
		# commands = []
		# for journal in testJournals:
		# 	commands.append(['Revit', os.path.abspath(journal)])

		# processes = [subprocess.Popen(cmd, shell=True) for cmd in commands]
		# for p in processes:
		# 	p.wait()

		for journal in testJournals:
			print 'running ' + journal
			run_cmd( ['Revit', os.path.abspath(journal)] )

	# aggregate results
	# results = [os.path.join(dirpath, f)
 #    	for dirpath, dirnames, files in os.walk(testsPath)
 #    	for f in fnmatch.filter(files, '*_result.xml')]
	results = glob.glob('*_result.xml')

	passCount = 0
	failCount = 0
	inconclusiveCount = 0

	for result in results:
		tree = ET.parse(result)
		root = tree.getroot()
		for member in root.iter('DynamoRevitTest'):
			testName = member.get('TestName')
			
			result_type = member.find('ResultType').text
			if result_type == "Pass":
				passCount +=1
			elif result_type == "Fail":
				failCount +=1
			elif result_type == "Inconclusive":
				inconclusiveCount +=1

			message_element = member.find('Message')
			message = ''
			if message_element is not None:
				message = message_element.text
			print testName + ':' + result_type
			if message is not '':
				print '\t' + message

	# cleanup results files and journals created during testing
	print 'cleaning journal files...'
	# runJournals = [os.path.join(dirpath, f)
 #    	for dirpath, dirnames, files in os.walk(testsPath)
 #    	for f in fnmatch.filter(files, 'journal.*')]
 	runJournals = glob.glob('journal.*')

	for journal in runJournals:
		os.remove(journal)

	print 'cleaning results files...'
	# results = [os.path.join(dirpath, f)
 #    	for dirpath, dirnames, files in os.walk(testsPath)
 #    	for f in fnmatch.filter(files, '*_result.xml')]

	for result in results:
		os.remove(result)

	end_time = time.time()
	ellapsed = end_time-start_time

	parsed_results = {}
	parsed_results['tests_run'] = len(testJournals)
	parsed_results['successes'] = passCount
	parsed_results['failures'] = failCount
	parsed_results['time'] = ellapsed
	parsed_results['inconclusives'] = inconclusiveCount

	summary = "Revit test summary: {0} Pass, {1} Fail, {2} Inconclusive, {3} total time.".format(passCount, failCount, inconclusiveCount, ellapsed)

	# send an email with the results
	send_email('Dynamo Revit Test Summary', summary, 'ian.keough@autodesk.com', 'ian.keough@autodesk.com')

	return parsed_results

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

if __name__ == "__main__":
	main()