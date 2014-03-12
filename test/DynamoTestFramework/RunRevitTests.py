import sys
import os
import fnmatch
import subprocess as sub
import threading
import xml.etree.ElementTree as ET
import argparse
import time
import glob
import smtplib
import string

class Test:
	def __init__(self, testName, fixtureName, testAssembly, resultsPath, modelPath, pluginGUID, pluginClass, runDynamo, debug):
		self.testName = testName
		self.fixtureName = fixtureName
		self.testAssembly = testAssembly
		self.resultsPath = resultsPath
		self.modelPath = modelPath
		self.pluginGUID = pluginGUID
		self.pluginClass = pluginClass
		self.runDynamo = runDynamo
		self.debug = debug
	def __repr__(self):
		return "<Test name:%s fixture:%s assembly:%s resultsPath:%s modelPath:%s pluginGUID:%s pluginClass:%s runDynamo:%s>" % (self.testName, self.fixtureName, self.testAssembly, self.resultsPath, self.modelPath, self.pluginGUID, self.pluginClass, self.runDynamo)

#http://stackoverflow.com/questions/4158502/python-kill-or-terminate-subprocess-when-timeout
class RunCmd(threading.Thread):
	def __init__(self, cmd, timeout):
		threading.Thread.__init__(self)
		self.cmd = cmd
		self.timeout = timeout

	def run(self):
		self.p = sub.Popen(self.cmd)
		self.p.wait()

	def Run(self):
		self.start()
		self.join(self.timeout)

		if self.is_alive():
			print 'Test timed out.'
			self.p.terminate()      #use self.p.kill() if process needs a kill -9
			self.join()

def main():

	#http://stackoverflow.com/questions/7427101/dead-simple-argparse-example-wanted-1-argument-3-results
	parser = argparse.ArgumentParser(description='Run Revit Tests')
	parser.add_argument('-n','--name', help='Name of a test to run.', required=False)
	parser.add_argument('-f','--fixture', help='Name of fixture to run.', required=False)
	parser.add_argument('-r','--results', nargs='?', const=1, default='DynamoTestResults.xml', help='Output location of the results file.', required=True)
	parser.add_argument('-i', '--input', help='An input xml specifying tests to be run.', required = True)
	parser.add_argument('-d', '--debug', help='Whether to run the tests in debug mode.', required = False)
	args = vars(parser.parse_args())

	tests = []
	resultsPath = os.path.abspath(args['results'])
	print resultsPath

	fixture = ''
	name=''
	debug=False

	if args['fixture'] is not None:
		fixture = args['fixture']

	if args['name'] is not None:
		name = args['name']

	if args['debug'] is not None:
		debug = args['debug']

	tests = parse_input_file(args['input'], resultsPath, fixture, name, debug)

	#cleanup existing results file
	if os.path.exists(args['results']):
		os.remove(args['results'])

	run_tests(tests, debug)

 	#cleanup journal files created by Revit
	journals = glob.glob("journal.*.txt")
	for journal in journals:
		os.remove(journal)

	sys.exit(0)

def run_tests(tests, debug):

	for test in tests:
		#Generate a temporary journal file
		journal = generate_journal_file(test)

		#Run Revit passing the journal file as a parameter
		print 'running ' + test.fixtureName + ':' + test.testName

		if debug:
			#Run command with no timeout.
			run_cmd(['Revit', os.path.abspath(journal)])
		else:
			#Run command with a two minute time-out.
			RunCmd(['Revit', os.path.abspath(journal)], 120 ).Run()

		#Cleanup temporary journal file
	 	#os.remove(journal)

def parse_input_file(inputFile, resultsPath, requestedFixture, requestedTest, debug):
	tests = []
	tree = ET.parse(inputFile)
	testRoot = tree.getroot()
	pluginClass = testRoot.get('pluginClass')
	pluginGUID = testRoot.get('pluginGUID')
	for testAssembly in testRoot.iter('testAssemblies'):
		for testAssembly_data in testAssembly.findall('testAssembly'):
			assemblyName = testAssembly_data.get('name')
			for fixture in testAssembly_data.findall('testFixture'):
				fixtureName = fixture.get('name')
				# if we've passed in the name of a fixture
				# and this fixture is not it, then do not process
				if len(requestedFixture)>0 and fixtureName != requestedFixture:
					continue
				for test in fixture.findall('test'):
					testName = test.get('name')
					runDynamo = test.get('runDynamo')
					modelPath = test.get('modelPath')
					if len(requestedTest)>0 and testName != requestedTest:
						continue
					test = Test(testName, fixtureName, assemblyName, resultsPath, modelPath, pluginGUID, pluginClass,runDynamo,debug)
					# print repr(test)
					tests.append(test)
	return tests

def run_cmd( args, printOutput = True, cwd = None ):	
	p = sub.Popen(args, shell=True, stdout=sub.PIPE, stderr=sub.STDOUT, cwd = cwd)
	
	out = ''

	while True:
	    line = p.stdout.readline()
	   
	    if not line:
	        break
	    out = out + line
	    print ">>> " + line.rstrip()
		
	return out

def generate_journal_file(test):
	
	currPath = os.getcwd()

	scriptPath = os.path.dirname(os.path.realpath(__file__))
	os.chdir(scriptPath)

	# read the template file
	template = glob.glob("JournalTemplate.txt")[0]

	s =''
	with open(template, 'r') as templateFile:
		s = string.Template(templateFile.read())
	templateFile.closed

	# change back to the test directory
	os.chdir(os.path.abspath(currPath))

	content = s.substitute(testName=test.testName, fixtureName=test.fixtureName, testAssembly=test.testAssembly, 
		resultsPath=test.resultsPath, modelName=test.modelPath, pluginGUID=test.pluginGUID, pluginClass=test.pluginClass, runDynamo=test.runDynamo, debug=test.debug)

	# save the content to a file
	journal = "DynamoTestFrameworkTmp.txt"
	with open(journal, 'w') as f:
		f.write(content)
	f.closed

	return journal

if __name__ == "__main__":
	main()

