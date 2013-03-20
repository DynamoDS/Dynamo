import os
from os import path
import re
from pprint import pprint
import itertools

class PortData(object):
    def __init__(self, nick, desc, type):
        self.name = nick
        self.description = desc
        self.type = type
        
    def __str__(self):
        return "**" + self.name + "** *(" + self.type + ")* - " + self.description

class Node(object):
    def __init__(self, name, desc, cat, ins, out):
        self.name = name
        self.description = desc
        self.category = cat
        self.inputs = ins
        self.output = out
    
    def __str__(self):
        return ("##" + self.name + "\n"
            + "###Description\n" 
            + self.description + "\n\n"
            + (("###Inputs\n" + "\n".join(["  * " + str(input) for input in self.inputs])) if len(self.inputs) else "") + "\n\n"
            + "###Output\n"
            + "  * " + str(self.output) + "\n"
        )
    
    __repl__ = __str__
    
    
CATEGORIES = {
    'BuiltinNodeCategories.BOOLEAN': "Boolean",
    'BuiltinNodeCategories.LIST': "List",
    'BuiltinNodeCategories.COMPARISON': "Logic",
    'BuiltinNodeCategories.MATH': "Math",
    'BuiltinNodeCategories.MISC': "Miscellaneous",
    'BuiltinNodeCategories.PRIMITIVES': "Primitives",
    'BuiltinNodeCategories.REVIT': "Revit",
    'BuiltinNodeCategories.ANALYSIS': "Analysis"
}

def getCat(key):
    if key in CATEGORIES:
        return CATEGORIES[key]
    else:
        return key
        
cwd = '../../src/'   # relative path

# generator nested list comprehension - returns a generator which yields all of the files in a folder ending with .cs and actually a file
file_array = ( [path.join(root, f) for f in files if path.isfile(path.join(root, f)) and path.splitext(f)[1] == '.cs'] for root, dirs, files in os.walk(cwd) )

# the * operator turns an array in to a list of args for the function
files = itertools.chain(*file_array)

nodes = []

for file in files:

    with open(file) as f:
        name = ""
        desc = ""
        cat = ""
        inputs = []
        output = None

        for line in f:

            line = line.strip()
            if not len(line): continue
            
            if not len(name): 
                search = re.search(r'\[NodeName\("([^"]+)"\)\]', line)
                if search:
                    name = search.group(1)
            else:
                if not len(desc):
                    search = re.search(r'\[NodeDescription\("([^"]+)"\)\]', line)
                    if search:
                        desc = search.group(1)
                        continue
                if not len(cat):
                    search = re.search(r'\[NodeCategory\(([^"]+)\)\]', line)
                    if search:
                        cat = getCat(search.group(1))
                        continue
                search = re.search(r'(?:this\s*\.\s*)?InPortData\s*\.\s*Add\s*\(\s*new\s*PortData\s*\(\s*"([^"]*)"\s*,\s*"([\s\w]*)"\s*,\s*typeof\s*\(\s*(\w+)\s*\)\s*\)\s*\)\s*;', line)
                if search:
                    inputs.append(PortData(*search.group(1, 2, 3)))
                    continue
                search = re.search(r'(?:this\s*\.\s*)?OutPortData\s*=\s*new\s*PortData\s*\(\s*"([^"]*)"\s*,\s*"([^"]*)"\s*,\s*typeof\s*\(\s*(\w+)\s*\)\s*\)\s*;', line)
                if search:
                    output = PortData(*search.group(1, 2, 3))
                    continue
                search = re.search(r'NodeUI\.RegisterAllPorts\s*\(\s*\)\s*;', line) 
                if search:
                    nodes.append(Node(name, desc, cat, inputs, output))
                    name = ""
                    desc = ""
                    cat = ""
                    inputs = []
                    output = None
                    
nodeDict = {}
for node in nodes:
    cat = node.category
    if cat not in nodeDict:
        nodeDict[cat] = []
    nodeDict[cat].append(node)
    
# print(nodeDict)

resultDict = {}
for key in sorted(list(nodeDict.keys())):
    catnodes = nodeDict[key]
    resultDict[key] = "\n\n".join([str(node) for node in sorted(catnodes, key=lambda x: x.name)])

if not path.exists('out'):
    os.makedirs('out')
    
for cat, text in resultDict.iteritems():
    with open("out/Category" + cat.replace(' ', '') + ".md", 'w') as f:
        f.write(text)