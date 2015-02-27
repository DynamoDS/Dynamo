ABOUT

build_and_test.py is a python script in scripts/easybuild that makes it easy to build Dynamo without having Visual Studio.  
It just fires up MSBuild, builds both release and debug, and runs the core unit tests.  Finally, it prints out the results 
in a more digestible form.


USAGE

At the command line, you should be able to run:

	build_and_test.py

You can get help by typing:

	build_and_test.py –h

This will print out additional command line arguments.  


DEPENDENCIES

You’ll need the following installed to use it:

	python - http://www.python.org/download/releases/2.7/
	NUnit - http://www.nunit.org/index.php?p=download
	MSBuild – http://www.microsoft.com/en-us/download/details.aspx?id=8279
