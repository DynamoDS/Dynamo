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
		'System.Collections.Generic.IList{Autodesk.Revit.DB.ElementId}':'lst',
		'Autodesk.Revit.DB.SketchPlane':'sp',
		'Autodesk.Revit.DB.HorizontalAlign':'ha',
		'Autodesk.Revit.DB.ModelTextType':'mtt',
		'Autodesk.Revit.DB.Electrical.ElectricalSystemType':'ett',
		'Autodesk.Revit.DB.Plumbing.PipeSystemType':'pst',
		'Autodesk.Revit.DB.Mechanical.DuctSystemType':'dst',
		'Autodesk.Revit.DB.DimensionType':'dt',
		'Autodesk.Revit.DB.Arc':'arc',
		'Autodesk.Revit.DB.DimensionType':'dimt',
		'Autodesk.Revit.DB.ReferenceArray':'refa',
		'Autodesk.Revit.DB.Form':'frm',
		'Autodesk.Revit.DB.ReferenceArrayArray':'arar',
		'Autodesk.Revit.DB.SweepProfile':'swpp',
		'Autodesk.Revit.DB.ProfilePlaneLocation':'ppl',
		'Autodesk.Revit.DB.TextNoteLeaderTypes':'tnlts',
		'Autodesk.Revit.DB.TextNoteLeaderStyles':'tnls',
		'Autodesk.Revit.DB.TextAlignFlags':'tafs',
		'Autodesk.Revit.DB.MEPCurve':'mepcrv',
		'Autodesk.Revit.DB.Connector':'con',
		'Autodesk.Revit.DB.Plumbing.FlexPipeType':'fpt',
		'Autodesk.Revit.DB.Plumbing.PipeType':'pt'
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
		'System.Collections.Generic.List{Autodesk.Revit.Creation.TextNoteCreationData}':'List<Autodesk.Revit.Creation.TextNoteCreationData>',
		'System.Collections.Generic.List{Autodesk.Revit.Creation.FamilyInstanceCreationData}':'List<Autodesk.Revit.Creation.FamilyInstanceCreationData>'
	}.get(x,x).replace('@','')

def write_node_attributes(node_name, summary, f):
	node_attributes = ['\t[NodeName("Revit ' + node_name + '")]\n',
		'\t[NodeCategory(BuiltinNodeCategories.REVIT_API)]\n',
		'\t[NodeDescription("' + summary + '")]\n']
	f.writelines(node_attributes)

def write_node_constructor(node_name, method_params, param_descriptions, summary, f, required_types):
	f.write('\t\tpublic Revit_' + node_name + '()\n')
	f.write('\t\t{\n')

	i=0
	for param_description in param_descriptions:
		param_description = param_descriptions[i].text.encode('utf-8').strip().replace('\n','').replace('\"','\\"')
		f.write('\t\t\tInPortData.Add(new PortData(\"'+match_inport_type(method_params[i])+'\", \"' + param_description + '\",typeof(object)));\n')

		if method_params[i] not in required_types and 'Autodesk.Revit.DB' in method_params[i]:
			required_types.append(method_params[i])

		i += 1

	f.write('\t\t\tOutPortData.Add(new PortData(\"out\",\"'+summary+'\",typeof(object)));\n')
	f.write('\t\t\tNodeUI.RegisterAllPorts();\n')
	f.write('\t\t}\n')

def write_node_evaluate(method_call_prefix, methodCall, method_params, f):
	f.write('\t\tpublic override Value Evaluate(FSharpList<Value> args)\n')
	f.write('\t\t{\n')

	# for each incoming arg, cast it to the matching param
	i = 0
	argList = []
	for param in method_params:
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

	if method_call_prefix == 'dynRevitSettings.Doc.Document.':
		f.write('\t\t\tif (dynRevitSettings.Doc.Document.IsFamilyDocument)\n')
		f.write('\t\t\t{\n')
		f.write('\t\t\t\tvar result = ' + method_call_prefix + 'FamilyCreate.' + methodCall +  '(' + paramsStr + ');\n')
		f.write('\t\t\t\treturn Value.NewContainer(result);\n')
		f.write('\t\t\t}\n')
		f.write('\t\t\telse\n')
		f.write('\t\t\t{\n')
		f.write('\t\t\t\tvar result = ' + method_call_prefix + 'Create.' + methodCall +  '(' + paramsStr + ');\n')
		f.write('\t\t\t\treturn Value.NewContainer(result);\n')
		f.write('\t\t\t}\n')
	else:
		f.write('\t\t\tvar result = ' + method_call_prefix + methodCall +  '(' + paramsStr + ');\n')
		f.write('\t\t\treturn Value.NewContainer(result);\n')


	f.write('\t\t}\n')
	
wrapperPath = './DynamoRevitNodes.cs'
# create a new text file to hold our wrapped classes
try:
   with open(wrapperPath) as f: pass
except IOError as e:
   print 'Could not find existing wrapper path .'

f = open(wrapperPath, 'w')

# create a list to store the node
# names that have been created
node_names = []

# create a list to store necessary 
# types from Autodesk
required_types = []

array_types = []

using=[	
'using System;\n',
'using System.Collections.Generic;\n',
'using System.Linq;\n',
'using Autodesk.Revit.DB;\n',
'using Autodesk.Revit;\n',
'using Dynamo.Controls;\n',
'using Dynamo.Utilities;\n',
'using Dynamo.Connectors;\n',
'using Dynamo.Revit;\n',
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
		elif "Autodesk.Revit.Creation.ItemFactoryBase" in member_name:
			method_call_prefix = 'dynRevitSettings.Doc.Document.'
		else:
			continue

		summary = member_data.find('summary').text.replace('\n','')
		paramDescriptions = member_data.findall('param')

		# we start with something like M:Autodesk.Revit.Creation.Application.NewPoint(Autodesk.Revit.DB.XYZ)
		methodDefinition = member_name.split(':')[1]	#Autodesk.Revit.Creation.Application.NewPoint(Autodesk.Revit.DB.XYZ)

		#print member_name
		if not "New" in methodDefinition:
			continue

		# take something like M:Autodesk.Revit.Creation.Application.NewPoint(Autodesk.Revit.DB.XYZ)
		# if there is a parenthesis, we have parameters
		if "(" in methodDefinition:
			# print methodDefinition
			methodName = member_name.split('.')[4][3:].split('(')[0]	#Point
			methodCall = member_name.split('.')[4].split('(')[0]		#NewPoint
			methodParams = member_name.split('(')[1][:-1].split(',')	#Autodesk.Revit.DB.XYZ
			fullMethodCall = member_name.split(':')[1].split('(')[0]	#Autodesk.Revit.Creation.Application.NewPoint
		else:
			# print methodDefinition
			methodName = member_name.split('.')[4][3:]	#Point
			methodCall = member_name.split('.')[4]		#NewPoint
			methodParams = []							#Autodesk.Revit.DB.XYZ
			fullMethodCall = member_name.split(':')[1]	#Autodesk.Revit.Creation.Application.NewPoint

		# if the class name already exists
		# append an integer to make it unique
		methodNameStub = methodName
		while methodName in node_names:
			node_name_counter += 1
			methodName = methodNameStub + '_' + str(node_name_counter)
		# else:
		node_name_counter = 0

		#store the node name
		node_names.append(methodName)

		write_node_attributes(methodName, summary, f)

		f.write('\tpublic class Revit_' + methodName + ' : dynRevitTransactionNodeWithOneOutput\n')
		f.write('\t{\n')

		#CONSTRUCTOR
		write_node_constructor(methodName, methodParams, paramDescriptions, summary,f, required_types)

		#EVALUATE
		write_node_evaluate(method_call_prefix, methodCall, methodParams, f)
		
		f.write('\t}\n')

		f.write('\n')
f.write('\t}\n')
f.close()

print node_names
#print required_types



