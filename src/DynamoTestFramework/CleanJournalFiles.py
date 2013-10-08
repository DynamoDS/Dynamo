import os
import fnmatch
import string 

def main():
	
	testsPath = os.path.join(os.path.dirname(__file__), "..\\test\\revit")

	# find all files to be cleaned
	journals = [os.path.join(dirpath, f)
    	for dirpath, dirnames, files in os.walk(testsPath)
    	for f in fnmatch.filter(files, '*.txt')]

	for journal in journals:
		# generate a new file name
		new_journal = journal.replace('.txt', '_clean.TEST')
		with open(new_journal, 'w') as j_new:
			with open(journal, 'r') as j:
				lines = j.readlines()
				start = False
				lastline = '_'
				for line in lines:
					if "Dim Jrn" in line:
						start = True
					if start is not True:
						#write all lines until we
						#start processing
						j_new.write(line)
						continue;

					if line == "":
						continue

					# remove leading  whitespace
					new_line = line.lstrip()
					# remove trailing  underscore
					# new_line = new_line.rstrip('_')
					# remove the line feed
					# new_line = new_line.rstrip('\n')

					if new_line == "":
						continue
					if new_line[0] != "'":
						j_new.write(new_line)

					lastline = line
			j.closed
		j_new.closed

if __name__ == "__main__":
	main()