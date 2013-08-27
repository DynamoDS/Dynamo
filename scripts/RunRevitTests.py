import os
import fnmatch
import subprocess
import xml.etree.ElementTree as ET

def main():
	# find all the journal files
	# journal files will end in .txt
	# these should be the only .txt files in the directory
	testsPath = os.path.join(os.path.dirname(__file__), "..\\test\\revit")
	#print(testsPath)

	testJournals = [os.path.join(dirpath, f)
    	for dirpath, dirnames, files in os.walk(testsPath)
    	for f in fnmatch.filter(files, '*.txt')]

	print testJournals

	# run all tests
	print 'running revit tests....'
	for journal in testJournals:
		print 'running ' + journal
		run_cmd( ['Revit', journal] )

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
	print 'cleaning journal files...'
	runJournals = [os.path.join(dirpath, f)
    	for dirpath, dirnames, files in os.walk(testsPath)
    	for f in fnmatch.filter(files, 'journal.*')]

	for journal in runJournals:
		os.remove(journal)

	# print 'cleaning results files...'
	# results = [os.path.join(dirpath, f)
 #    	for dirpath, dirnames, files in os.walk(testsPath)
 #    	for f in fnmatch.filter(files, '*_result.xml')]

	# for result in results:
	# 	os.remove(result)

def run_cmd( args, printOutput = True, cwd = None ):	
	processes = subprocess.Popen(args, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd = cwd)
	
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