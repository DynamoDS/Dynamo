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
        'System.Boolean': 'Value.Number',
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
		'Autodesk.Revit.DB.Element':'el',
		'System.Collections.Generic.IList{Autodesk.Revit.DB.ElementId}':'lst'
	}.get(x,'val')

def convert_param(x):
	return{
		'System.Collections.Generic.IList{Autodesk.Revit.DB.XYZ}':'List<Autodesk.Revit.DB.XYZ>',
		'System.Collections.Generic.ICollection{Autodesk.Revit.DB.ElementId}':'List<Autodesk.Revit.DB.ElementId>',
		'System.Collections.Generic.IList{System.Double}':'List<double>',
		'System.Collections.Generic.List{Autodesk.Revit.Creation.AreaCreationData}':'List<Autodesk.Revit.Creation.AreaCreationData>',
		'System.Collections.Generic.List{Autodesk.Revit.Creation.RoomCreationData}':'List<Autodesk.Revit.Creation.RoomCreationData>',
		'System.Collections.Generic.List{Autodesk.Revit.Creation.ProfiledWallCreationData}':'List<Autodesk.Revit.Creation.ProfiledWallCreationData>',
		'System.Collections.Generic.List{Autodesk.Revit.Creation.RectangularWallCreationData}':'List<Autodesk.Revit.Creation.RectangularWallCreationData>',
	}.get(x,x).replace('@','')

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
'using Autodesk.Revit.DB;\n',
'using Autodesk.Revit;\n',
'using Dynamo.Controls;\n',
'using Dynamo.Utilities;\n',
'using Dynamo.Connectors;\n',
'using Dynamo.FSchemeInterop;\n',
'using Dynamo.FSchemeInterop.Node;\n',
'using Microsoft.FSharp.Collections;\n',
'using Value = Dynamo.FScheme.Value;\n']
f.writelines(using)

f.write('namespace Dynamo.Nodes\n')
f.write('{\n')
for member in root.iter('members'):
	node_name_counter = 0
	for member_data in member.findall('member'):

		member_name = member_data.get('name')

		#Application.Create
		#Document.Create
		#Document.FamilyCreate
		method_call_prefix = ''
		if "Autodesk.Revit.Creation.Application" in member_name:
			method_call_prefix = 'dynRevitSettings.Revit.Application.Create.'
		elif "Autodesk.Revit.Creation.FamilyItemFactory" in member_name:
			method_call_prefix  = 'dynRevitSettings.Doc.Document.FamilyCreate.'
		elif "Autodesk.Revit.Creation.Document" in member_name:
			method_call_prefix = 'dynRevitSettings.Doc.Document.Create.'
		else:
			continue

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
		fullMethodCall = member_name.split(':')[1].split('(')[0]

		# if the class name already exists
		# append an integer to make it unique
		if className in node_names:
			node_name_counter += 1
			className = className + '_' + str(node_name_counter)
		else:
			node_name_counter = 0

		node_names.append(className)

		class_attributes = ['\t[NodeName("Revit ' + className + '")]\n',
		'\t[NodeCategory(BuiltinNodeCategories.REVIT_API)]\n',
		'\t[NodeDescription("' + summary + '")]\n']
		f.writelines(class_attributes)
		f.write('\tpublic class Revit_' + className + ' : dynNodeWithOneOutput\n')
		f.write('\t{\n')
		f.write('\t\tpublic Revit_' + className + '()\n')
		f.write('\t\t{\n')
		for param in methodParams:
			f.write('\t\t\tInPortData.Add(new PortData(\"'+match_inport_type(param)+'\", \"' + convert_param(param) + '\",typeof(object)));\n')
		f.write('\t\t\tOutPortData.Add(new PortData(\"out\",\"'+summary+'\",typeof(object)));\n')
		f.write('\t\t\tNodeUI.RegisterAllPorts();\n')
		f.write('\t\t}\n')
		f.write('\t\tpublic override Value Evaluate(FSharpList<Value> args)\n')
		f.write('\t\t{\n')

		# for each incoming arg, cast it to the matching param
		i = 0
		argList = []
		for param in methodParams:
			# there is no boolean type in FScheme
			# convert a number type to a boolean
			if param == 'System.Boolean':
				f.write('\t\t\tvar arg' + str(i) + '=Convert.ToBoolean(((' + match_param(param) + ')args[' + str(i) +']).Item);\n')
			else:
				f.write('\t\t\tvar arg' + str(i) + '=(' + convert_param(param) + ')((' + match_param(param) + ')args[' + str(i) +']).Item;\n')

			if '@' in param:
				argList.append('out arg' + str(i))
			else:
				argList.append('arg' + str(i))
			i+=1
		paramsStr = ",".join(argList)
		f.write('\t\t\tvar result = ' + method_call_prefix + methodCall +  '(' + paramsStr + ');\n')
		f.write('\t\t\treturn Value.NewContainer(result);\n')
		f.write('\t\t}\n')
		f.write('\t}\n')
		f.write('\n')
f.write('\t}\n')
f.close()

print node_names



