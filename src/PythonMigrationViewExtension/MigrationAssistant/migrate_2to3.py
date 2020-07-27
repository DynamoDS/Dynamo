from lib2to3.pgen2.parse import ParseError
from lib2to3.refactor import RefactoringTool, get_fixers_from_package
import zipfile
import os.path
from pathlib import Path
import sys

def transform(source):
    python_zip_path = str(Path(os.path.dirname(os.__file__)).parents[0])
    python_zip_file = get_zip_file(python_zip_path)
    python_zip_folder = zipfile.ZipFile(os.path.join(python_zip_path, python_zip_file))
    fixers = get_all_fixers_from_zipfolder(python_zip_folder)

    refactoring_tool = RefactoringTool(fixers)

    return refactor_script(source, refactoring_tool)

def refactor_script(source_script, refactoring_tool):
    # lib2to3 likes a newline at the end
    source_script += '\n'
    try:
        tree = refactoring_tool.refactor_string(source_script, 'script')
    except ParseError as e:
        if e.msg != 'bad input' or e.value != '=':
            raise
        tree = refactoring_tool.refactor_string(source_script, self.pathname)
    return str(tree)[:-1] # remove added newline 

     
def get_zip_file(path):
    zip_file = ""
    for f in os.listdir(path):
        if f.startswith("python") and f.endswith(".zip"):
            zip_file = f
            break
    return zip_file

# because we are using an embedded version of python, we cant get 2to3's fixers
# like we normally would useing "get_fixers_from_package('lib2to3.fixes')" 
# this is a known limitation of 2to3. Instead we are getting the embedded python.zip
# and manually navigating to the 2to3 fixers from there.
def get_all_fixers_from_zipfolder(folder):
    fixers = []
    files = folder.filelist
    for f in files:
        if "fix_" not in f.filename:
            continue
        fixers.append(f.filename.replace('/', '.')[:-4])
    return fixers

output = transform(code)
