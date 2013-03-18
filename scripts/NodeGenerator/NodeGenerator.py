# Node generator generates Dynamo nodes from the Revit API help documentation
# Classes take the form
# [ElementName("<name from xml>")]
# [ElementCategory(BuiltinElementCategories.REVIT_GEOMETRY)]
# [ElementDescription("<description from xml")]
# [RequiresTransaction(false)]
# public Class <node name> : dynNode
# {
#	public <node name>()
#	{
#		InPortData.Add(new PortData("xyz", "The point(s) from which to create reference points.", typeof(XYZ)));
#       OutPortData = new PortData("pt", "The Reference Point(s) created from this operation.", typeof(ReferencePoint));
#
#       base.RegisterInputsAndOutputs();
#	}
#	
#	public override Expression Evaluate(FSharpList<Expression> args)
#   {
#		return Express.NewContainer(result)
#	}
# }

import xml.etree.ElementTree as ET

tree = ET.parse('RevitAPI.xml')
root = tree.getroot()

def match_param(x):
    return {
        'System.Double': 'Value.Number',
        'System.Boolean': 'Value.Boolean',
        'System.Int32':'Value.Number',
        'System.String':'Value.String'
        }.get(x, 'Value.Container') 

def match_inport_type(x):
	return{
		'System.Double':'n',
		'System.Int32':'i',
		'System.String':'s',
		'System.Boolean':'b',
		'Autodesk.Revit.DB.UV':'uv',
		'Autodesk.Revit.DB.XYZ':'xyz',
		'Autodesk.Revit.DB.Plane':'p',
		'Autodesk.Revit.DB.CurveArray':'crvs',
		'Autodesk.Revit.DB.CurveArrArray':'crvs',
		'Autodesk.Revit.DB.CategorySet':'cats',
		'Autodesk.Revit.DB.Level':'l',
		'Autodesk.Revit.DB.FamilySymbol':'fs',
		'Autodesk.Revit.DB.Curve':'crv',
		'Autodesk.Revit.DB.Structure.StructuralType':'st',
		'System.Collections.Generic.IList{Autodesk.Revit.DB.XYZ}':'lst',
		'System.Collections.Generic.IList{System.Double}':'lst',
		'Autodesk.Revit.DB.DoubleArray':'arr',
		'Autodesk.Revit.DB.Reference':'ref',
		'Autodesk.Revit.DB.PointLocationOnCurve':'loc',
		'Autodesk.Revit.DB.ViewPlan':'v',
		'Autodesk.Revit.DB.View':'v',
		'Autodesk.Revit.DB.WallType':'wt',
		'Autodesk.Revit.DB.Face':'f',
		'Autodesk.Revit.DB.Line':'crv',
		'Autodesk.Revit.DB.Element':'el'
	}.get(x,'val')

def convert_param(x):
	return{
		'System.Collections.Generic.IList{Autodesk.Revit.DB.XYZ}':'List<Autodesk.Revit.DB.XYZ>',
		'System.Collections.Generic.IList{System.Double}':'List<double>'
	}.get(x,x)

wrapperPath = './DynamoRevitNodes.cs'
# create a new text file to hold our wrapped classes
try:
   with open(wrapperPath) as f: pass
except IOError as e:
   print 'Could not find existing wrapper path .'

f = open(wrapperPath, 'w')

# define a dictionary to store the node
# names that have been created
node_names = []

using=[	
'using System;\n',
'using System.Collections.Generic;\n',
'using Dynamo.Elements;\n',
'using System.IO;\n',
'using System.Linq;\n',
'using System.Threading;\n',
'using System.Windows;\n',
'using System.Windows.Controls;\n',
'using System.Windows.Forms;\n',
'using System.Windows.Media;\n',
'using System.Xml;\n',
'using System.Web;\n',
'using Dynamo.Connectors;\n',
'using Dynamo.Controls;\n',
'using Dynamo.FSchemeInterop;\n',
'using Dynamo.FSchemeInterop.Node;\n',
'using Dynamo.Utilities;\n',
'using Microsoft.FSharp.Collections;\n',
'using Expression = Dynamo.FScheme.Expression;\n',
'using TextBox = System.Windows.Controls.TextBox;\n',
'using System.Diagnostics.Contracts;\n',
'using System.Text;\n',
'using System.Windows.Input;\n',
'using System.Windows.Data;\n',
'using System.Globalization;\n',
'using Autodesk.Revit.Creation.Application;\n']
f.writelines(using)

f.write('namespace Dynamo.Elements\n')
f.write('{\n')
for member in root.iter('members'):
	node_name_counter = 0
	for member_data in member.findall('member'):

		member_name = member_data.get('name')
		if not "Autodesk.Revit.Creation.Application" in member_name: continue

		summary = member_data.find('summary').text.replace('\n','')
		methodDefinition = member_name.split(':')[1]

		# don't use method definitions that don't have the form
		# someMethodName(param, param, param)
		if not "(" in methodDefinition: continue

		#print member_name

		# take something like M:Autodesk.Revit.Creation.Application.NewPoint(Autodesk.Revit.DB.XYZ)
		# and turn it into 'Point'
		className = member_name.split('.')[4][3:].split('(')[0]
		methodCall = member_name.split('.')[4].split('(')[0]
		methodParams = member_name.split('(')[1][:-1].split(',')

		# if the class name already exists
		# append an integer to make it unique
		if className in node_names:
			node_name_counter += 1
			className = className + '_' + str(node_name_counter)
		else:
			node_name_counter = 0

		node_names.append(className)

		class_attributes = ['\t[ElementName("' + className + '")]\n',
		'\t[ElementCategory(BuiltinElementCategories.AUTO)]\n',
		'\t[ElementDescription("' + summary + '")]\n',
		'\t[RequiresTransaction(false)]\n']
		f.writelines(class_attributes)
		f.write('\tpublic class ' + className + ' : dynNode\n')
		f.write('\t{\n')
		f.write('\t\tpublic ' + className + '()\n')
		f.write('\t\t{\n')
		for param in methodParams:
			f.write('\t\t\tInPortData.Add(new PortData(\"'+match_inport_type(param)+'\", \"' + convert_param(param) + '\",typeof(object)));\n')
		f.write('\t\t\tOutPortData.Add(new PortData(\"out\",\"'+summary+'\",typeof(object)));\n')
		f.write('\t\t\tNodeUI.RegisterAllPorts();\n')
		f.write('\t\t}\n')
		f.write('\t\tpublic override Expression Evaluate(FSharpList<Expression> args)\n')
		f.write('\t\t{\n')

		# for each incoming arg, cast it to the matching param
		i = 0
		argList = []
		for param in methodParams:
			f.write('\t\t\tvar arg' + str(i) + '=(' + convert_param(param) + ')((' + match_param(param) + ')args[' + str(i) +']).Item;\n')
			argList.append('arg' + str(i))
			i+=1
		paramsStr = ",".join(argList)
		f.write('\t\t\tvar result = ' + methodCall + '(' + paramsStr + ');\n')
		f.write('\t\t\treturn Value.NewContainer(result);\n')
		f.write('\t\t}\n')
		f.write('\t}\n')
		f.write('\n')
f.write('\t}\n')
f.close()

print node_names


