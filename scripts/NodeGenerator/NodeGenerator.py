import xml.etree.ElementTree as ET
import pprint

def main():
	tree = ET.parse('RevitAPI.xml')
	root = tree.getroot()

	wrapperPath = './DynamoRevitNodes.cs'
	categoriesPath = './DynamoRevitCategories.cs'

	# create a new text file to hold our wrapped classes
	try:
	   with open(wrapperPath) as f: pass
	except IOError as e:
	   print 'Could not find existing wrapper path .'

	f = open(wrapperPath, 'w')

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

	skip_list = [
	'Autodesk.Revit.DB.Transform.Identity',
	'Autodesk.Revit.DB.XYZ.BasisX',
	'Autodesk.Revit.DB.XYZ.BasisY',
	'Autodesk.Revit.DB.XYZ.BasisZ',
	'Autodesk.Revit.DB.XYZ.Zero',
	'Autodesk.Revit.DB.UV.BasisU',
	'Autodesk.Revit.DB.UV.BasisV',
	'Autodesk.Revit.DB.UV.Zero',
	]

	valid_namespaces = [
	'Autodesk.Revit.Creation.Application', 
	'Autodesk.Revit.Creation.FamilyItemFactory', 
	'Autodesk.Revit.Creation.Document', 
	'Autodesk.Revit.Creation.ItemFactoryBase',

	'Autodesk.Revit.DB.Curve',
	'Autodesk.Revit.DB.Arc',
	'Autodesk.Revit.DB.Ellipse',
	'Autodesk.Revit.DB.HermiteSpline',
	'Autodesk.Revit.DB.Line',
	'Autodesk.Revit.DB.NurbSpline',

	'Autodesk.Revit.DB.Face',
	'Autodesk.Revit.DB.ConicalFace',
	'Autodesk.Revit.DB.CylindricalFace',
	'Autodesk.Revit.DB.HermiteFace',
	'Autodesk.Revit.DB.RevolvedFace',
	'Autodesk.Revit.DB.RuledFace',

	'Autodesk.Revit.DB.GenericForm',
    'Autodesk.Revit.DB.Blend',
    'Autodesk.Revit.DB.Extrusion',
    'Autodesk.Revit.DB.Form',
    'Autodesk.Revit.DB.Revolution',
    'Autodesk.Revit.DB.Sweep',
    'Autodesk.Revit.DB.SweptBlend',

	'Autodesk.Revit.DB.GeometryObject',
    'Autodesk.Revit.DB.Edge',
    'Autodesk.Revit.DB.GeometryInstance',
    'Autodesk.Revit.DB.Mesh',
    'Autodesk.Revit.DB.Point',
    'Autodesk.Revit.DB.PolyLine',
    'Autodesk.Revit.DB.Profile',
    'Autodesk.Revit.DB.Solid',

    'Autodesk.Revit.DB.Instance',
    'Autodesk.Revit.DB.FamilyInstance',
    'Autodesk.Revit.DB.PointCloudInstance',

    'Autodesk.Revit.DB.DividedSurface',

    'Autodesk.Revit.DB.AdaptiveComponentFamilyUtils',
    'Autodesk.Revit.DB.AdaptiveComponentInstanceUtils',

    'Autodesk.Revit.DB.Transform',
    'Autodesk.Revit.DB.ModelArc',
    'Autodesk.Revit.DB.ModelCurve',
    'Autodesk.Revit.DB.ModelSpline',
    'Autodesk.Revit.DB.ModelHermiteSpline',
    'Autodesk.Revit.DB.ModelEllipse',
    'Autodesk.Revit.DB.ModelLine',

    'Autodesk.Revit.DB.XYZ',
    'Autodesk.Revit.DB.UV',

    'Autodesk.Revit.DB.Wall',
    'Autodesk.Revit.DB.Material',
    'Autodesk.Revit.DB.Level',
    'Autodesk.Revit.DB.Color',
    'Autodesk.Revit.DB.BoundingBoxUV',
    'Autodesk.Revit.DB.BoundingBoxXYZ',
	]

	revit_types = {}
	
	# create a list to store the node
	# names that have been created
	node_names = []

	read_types(root, revit_types, valid_namespaces)
	read_methods(root, revit_types, valid_namespaces, node_names, skip_list)
	read_properties(root, revit_types, valid_namespaces, skip_list)

	for key in revit_types.keys():
		revit_types[key].write(f);

	f.write('}\n')
	f.close()

	# create a new text file to hold our wrapped classes
	try:
	   with open(categoriesPath) as f: pass
	except IOError as e:
	   print 'Could not find existing categories path .'

	f = open(categoriesPath, 'w')
	using=[	
	'using System;\n']
	f.writelines(using)

	f.write('namespace Dynamo.Nodes\n')
	f.write('{\n')
	f.write('\tpublic static partial class BuiltinNodeCategories\n')
	f.write('\t{\n')

	for namespace in valid_namespaces:
		f.write('\t\tpublic const string ' + namespace.upper().replace('.','_') + ' = \"' + namespace + '\";\n')

	f.write('\t}\n')
	f.write('}\n')
	f.close();

class RevitType:
	def __init__(self, name, summary):
	    self.methods = []
	    self.properties = []
	    self.name = name
	    self.summary = summary

	def __str__(self):
		string = self.name + "\n"
		string += self.summary + "\n"
		string += "Methods:\n"
		for m in self.methods:
			string += str(m) + "\n"
		string += "Properties:\n"
		for p in self.properties:
			string += str(p) + "\n"
		return string

	def write(self, f):
		for m in self.methods:
			m.write(f)
		for p in self.properties:
			p.write(f)

class RevitMethod:
	def __init__(self, name, returns, summary, node_names):
		self.name = name
		self.returns = returns
		self.summary = summary
		self.parameters=[]
		self.isStatic = False
		self.isConstructor = False

		if '(' in self.name:
			self.type = self.name.split('(')[0].rsplit('.',1)[0]
		else:
			self.type = self.name.rsplit('.',1)[0]

		if '#ctor' in self.name:
			self.isConstructor = True;
			self.nickName = 'Revit_' + self.name.split('.')[-2]
		else:
			splits = self.name.split('.')
			self.nickName = 'Revit_' + splits[-2] + '_' + splits[-1]
			
		#append an index to the method
		#if a similar method exists
		methodNameStub = self.nickName
		node_name_counter = 0
		while self.nickName in node_names:
			node_name_counter += 1
			self.nickName = methodNameStub + '_' + str(node_name_counter)
		
		node_names.append(self.nickName)
		node_name_counter = 0

		if '(' in self.name:
			self.methodCall = self.name.split('(')[0].split('.')[-1]  	
		else:
			self.methodCall = self.name.split('.')[-1] 

		if self.isConstructor:
			self.methodCall = self.name.rsplit('.',2)[1]

		self.match_method_call()

	def __str__(self):
		string = "name:" + self.name + "\n"
		string += "returns:" + self.returns + "\n"
		string += "summary:" + self.summary + "\n"
		string += "parameters:\n"
		for p in self.parameters:
			string += p.key + ":" + p.value +"\n"
		return string

	def write(self, f):
		self.write_attributes(f)
		f.write('\tpublic class ' + self.nickName + ' : dynRevitTransactionNodeWithOneOutput\n')
		f.write('\t{\n')
		self.write_constructor(f)
		self.write_evaluate(f)
		f.write('\t}\n')
		f.write('\n')

	def write_attributes(self, f):
		node_attributes = ['\t[NodeName("' + self.nickName + '")]\n',
		'\t[NodeCategory(BuiltinNodeCategories.' + self.type.upper().replace('.','_') + ')]\n',
		'\t[NodeDescription("' + self.summary.encode('utf-8').strip().replace('\n','').replace('\"','\\"') + '")]\n']
		f.writelines(node_attributes)

	def write_constructor(self, f):
		f.write('\t\tpublic ' + self.nickName + '()\n')
		f.write('\t\t{\n')

		if not self.isStatic:
			f.write('\t\t\tInPortData.Add(new PortData(\"'+match_inport_type(self.type)+'\", \"' + self.type + '\",typeof(object)));\n')

		for param in self.parameters:
			param_description = param.description.encode('utf-8').strip().replace('\n','').replace('\"','\\"')
			f.write('\t\t\tInPortData.Add(new PortData(\"'+match_inport_type(param.param_type)+'\", \"' + param_description + '\",typeof(object)));\n')

		f.write('\t\t\tOutPortData.Add(new PortData(\"out\",\"'+self.summary.encode('utf-8').strip().replace('\n','').replace('\"','\\"')+'\",typeof(object)));\n')
		f.write('\t\t\tNodeUI.RegisterAllPorts();\n')
		f.write('\t\t}\n')

	def write_evaluate(self, f):
		f.write('\t\tpublic override Value Evaluate(FSharpList<Value> args)\n')
		f.write('\t\t{\n')

		# for each incoming arg, cast it to the matching param
		i = 0
		argList = []

		if not self.isStatic:
			f.write('\t\t\tvar arg' + str(i) + '=(' + self.type + ')DynamoTypeConverter.ConvertInput(args['+str(i)+'], typeof(' + self.type + '));\n')
			i+=1

		outMember = ''

		for param in self.parameters:
			param.write(i, outMember, argList, f)
			i+=1

		if len(self.parameters) > 0:
			paramsStr = '(' +  ",".join(argList) + ")"
		else:
			paramsStr = '()'

		# logic for testing if we're in a family document
		if self.method_call_prefix == 'dynRevitSettings.Doc.Document':
			f.write('\t\t\tif (dynRevitSettings.Doc.Document.IsFamilyDocument)\n')
			f.write('\t\t\t{\n')
			f.write('\t\t\t\tvar result = ' + self.method_call_prefix + '.FamilyCreate.' + self.methodCall + paramsStr + ';\n')
			if outMember != '':
				f.write('\t\t\t\treturn DynamoTypeConverter.ConvertToValue(' + outMember + ');\n')
			else:
				f.write('\t\t\t\treturn DynamoTypeConverter.ConvertToValue(result);\n')
			f.write('\t\t\t}\n')
			f.write('\t\t\telse\n')
			f.write('\t\t\t{\n')
			f.write('\t\t\t\tvar result = ' + self.method_call_prefix + '.Create.' + self.methodCall + paramsStr + ';\n')
			if outMember != '':
				f.write('\t\t\t\treturn DynamoTypeConverter.ConvertToValue(' + outMember + ');\n')
			else:
				f.write('\t\t\t\treturn DynamoTypeConverter.ConvertToValue(result);\n')
			f.write('\t\t\t}\n')
		else:
			f.write('\t\t\tvar result = ' + self.method_call_prefix + '.' + self.methodCall + paramsStr + ';\n')
			if outMember != '':
				f.write('\t\t\treturn DynamoTypeConverter.ConvertToValue(' + outMember + ');\n')
			else:
				f.write('\t\t\treturn DynamoTypeConverter.ConvertToValue(result);\n')

		f.write('\t\t}\n')
	
	def match_method_call(self):

		def_prefix = '((' + self.type + ')(args[0] as Value.Container).Item)'

		self.method_call_prefix = {
			"Autodesk.Revit.Creation.Application":'dynRevitSettings.Revit.Application.Create',
			"Autodesk.Revit.Creation.FamilyItemFactory":'dynRevitSettings.Doc.Document.FamilyCreate',
			"Autodesk.Revit.Creation.Document":'dynRevitSettings.Doc.Document.Create',
			"Autodesk.Revit.Creation.ItemFactoryBase":'dynRevitSettings.Doc.Document',
			"Autodesk.Revit.DB.AdaptiveComponentFamilyUtils":'Autodesk.Revit.DB.AdaptiveComponentFamilyUtils',
			"Autodesk.Revit.DB.AdaptiveComponentInstanceUtils":'Autodesk.Revit.DB.AdaptiveComponentInstanceUtils',
		}.get(self.type, def_prefix)

		if self.method_call_prefix is not def_prefix:
			self.isStatic = True

		print self.name
		if '.Create' in self.name:
			self.isStatic = True
			self.method_call_prefix = self.type

		if self.isConstructor:
			self.method_call_prefix = 'new ' + self.name.rsplit('.',2)[0]

class RevitProperty:
	def __init__(self, name, summary):
		self.name = name
		self.summary = summary
		self.parameters=[]
		self.isStatic = False
		self.type = self.name.split('(')[0].rsplit('.',1)[0]
		splits = self.name.split('(')[0].split('.')
		self.nickName = 'Revit_' + splits[-2] + '_' + splits[-1]

		if '(' in self.name:
			self.method_call = self.name.split('(')[0].split('.')[-1]  	
		else:
			self.method_call = self.name.split('.')[-1] 

	def __str__(self):
		string = self.name +"\n"
		string += self.summary +"\n"
		return string

	def write(self, f):
		self.write_attributes(f)
		f.write('\tpublic class ' + self.nickName + ' : dynRevitTransactionNodeWithOneOutput\n')
		f.write('\t{\n')
		self.write_constructor(f)
		self.write_evaluate(f)
		f.write('\t}\n')
		f.write('\n')

	def write_attributes(self,f):
		node_attributes = ['\t[NodeName("' + self.nickName + '")]\n',
		'\t[NodeCategory(BuiltinNodeCategories.' + self.type.upper().replace('.','_') + ')]\n',
		'\t[NodeDescription("' + self.summary.encode('utf-8').strip().replace('\n','').replace('\"','\\"') + '")]\n']
		f.writelines(node_attributes)

	def write_constructor(self, f):
		f.write('\t\tpublic ' + self.nickName + '()\n')
		f.write('\t\t{\n')

		f.write('\t\t\tInPortData.Add(new PortData(\"'+match_inport_type(self.type)+'\", \"' + self.type + '\",typeof(object)));\n')
		f.write('\t\t\tOutPortData.Add(new PortData(\"out\",\"'+self.summary.encode('utf-8').strip().replace('\n','').replace('\"','\\"')+'\",typeof(object)));\n')
		f.write('\t\t\tNodeUI.RegisterAllPorts();\n')
		f.write('\t\t}\n')

	def write_evaluate(self, f):
		f.write('\t\tpublic override Value Evaluate(FSharpList<Value> args)\n')
		f.write('\t\t{\n')

		# for each incoming arg, cast it to the matching param
		i = 0
		f.write('\t\t\tvar arg' + str(i) + '=(' + self.type + ')DynamoTypeConverter.ConvertInput(args['+str(i)+'], typeof(' + self.type + '));\n')
		i+=1
		f.write('\t\t\tvar result = ((' + self.type + ')(args[0] as Value.Container).Item).' + self.method_call +  ';\n')

		f.write('\t\t\treturn DynamoTypeConverter.ConvertToValue(result);\n')
		f.write('\t\t}\n')

class RevitParameter:
	def __init__(self, name, param_type, description):
		self.name = name
		self.param_type = param_type
		self.description = description
		self.isOut = False
		if '@' in self.param_type:	
			self.isOut = True

	def write(self, index, outMember, argList, f):
		f.write('\t\t\tvar arg' + str(index) + '=(' + convert_param(self.param_type).replace('@','') +')DynamoTypeConverter.ConvertInput(args[' + str(index) +'],typeof(' + convert_param(self.param_type).replace('@','') +'));\n')

		if self.isOut:
			argList.append('out arg' + str(index))
			outMember = 'arg' + str(index) #flag this out value so we can return it instead of the result
		else:
			argList.append('arg' + str(index))

def check_namespace(name, valid_namespaces):
	if '(' in name:
		check_name = name.split('(')[0].rsplit('.',1)[0].split(':')[1] 
	else:
		check_name = name.rsplit('.',1)[0].split(':')[1]

	for namespace in valid_namespaces:
		if namespace == check_name:
			return True
	return False

def read_types(root, revit_types, valid_namespaces):
	for member in root.iter('members'):
		for member_data in member.findall('member'):
			member_name = member_data.get('name')
			if "T:" in member_name:
				if check_namespace(member_name, valid_namespaces):
					read_type(member_data, revit_types)

def read_type(member_data, revit_types):
	name = member_data.get('name').split(':')[1]
	try:
		summary = member_data.find('summary').text.replace('\n','')
	except:
		summary = ''

	newType = RevitType(name, summary)
	revit_types[name] = newType

def read_methods(root, revit_types, valid_namespaces, node_names, skip_list):
	for member in root.iter('members'):
		for member_data in member.findall('member'):
			member_name = member_data.get('name')
			if "M:" in member_name:
				if check_namespace(member_name, valid_namespaces):
					read_method(member_data, revit_types, node_names,skip_list)

def read_properties(root, revit_types, valid_namespaces, skip_list):
	for member in root.iter('members'):
		for member_data in member.findall('member'):
			member_name = member_data.get('name')
			if "P:" in member_name:
				if check_namespace(member_name, valid_namespaces):
					read_property(member_data, revit_types, skip_list)

def read_method(member_data, revit_types, node_names, skip_list):

	method_name = member_data.get('name').split(':')[1] #take off the M:
	print method_name

	param_types = []
	if '(' in method_name:
		param_types = method_name.split('(')[1][:-1].split(',')
		method_name = method_name.split('(')[0]
		#print len(param_types)

	if method_name in skip_list:
		return

	try:
		summary = member_data.find('summary').text.replace('\n','')
	except:
		summary = ''

	try:
		returns = member_data.find('returns').text.replace('\n','')
	except:
		returns = ''

	#do not read void members for now
	if returns == '':
		return

	#do not read operator overloads for now
	if 'op_' in method_name:
		return

	newMethod = RevitMethod(method_name, summary, returns, node_names)

	params = member_data.findall('param')

	#the Revit API xml has methods where there are
	#more parameter descriptions than there are parameters
	#in this case, just return
	if len(param_types) != len(params):
		return
	
	paramCount = 0
	for param in params:
		param_name = param.get('name').replace('\n','')
		if param.text is None:
			param_description = ''
		else:
			param_description = param.text
		newParameter = RevitParameter(param_name, param_types[paramCount],param_description)
		newMethod.parameters.append(newParameter)
		paramCount += 1


	#the type name is the method name minus the
	#last segment after the '.'
	#for static methods, no type will exist in the
	#types dictionary yet, so you need to add one
	type_name = method_name.rsplit('.',1)[0]
	if type_name not in revit_types:
		revit_types[type_name] = RevitType(type_name, '')
	revit_types[type_name].methods.append(newMethod)

def read_property(member_data, revit_types, skip_list):
	property_name = member_data.get('name').split(':')[1] #take off the M:
	try:
		summary = member_data.find('summary').text.replace('\n','')
	except:
		summary = ''

	param_types = []
	if '(' in property_name:
		param_types = property_name.split('(')[1][:-1].split(',')
		property_name = property_name.split('(')[0]

	if property_name in skip_list:
		return

	params = member_data.findall('param')

	#don't wrap properties with parameters now
	if len(param_types) > 0:
		return

	#the Revit API xml has methods where there are
	#more parameter descriptions than there are parameters
	#in this case, just return
	if len(param_types) != len(params):
		return
	
	newProperty = RevitProperty(property_name, summary)

	paramCount = 0
	for param in params:
		param_name = param.get('name').replace('\n','')
		if param.text is None:
			param_description = ''
		else:
			param_description = param.text
		newParameter = RevitParameter(param_name, param_types[paramCount],param_description)
		newProperty.parameters.append(newParameter)
		paramCount += 1

	if paramCount > 0:
		newProperty.isStatic = True

	#the type name is the method name minus the
	#last segment after the '.'
	type_name = property_name.rsplit('.',1)[0]
	if type_name not in revit_types:
		revit_types[type_name] = RevitType(type_name, '')
	revit_types[type_name].properties.append(newProperty)

def match_param(x):
    return {
        'System.Double': 'Value.Number',
        'System.Boolean': 'Value.Number',
        'System.Int32':'Value.Number',
        'System.String':'Value.String',
        'Autodesk.Revit.DB.CurveArray':'Value.List',
        'Autodesk.Revit.DB.CurveArrArray':'Value.List',
        'Autodesk.Revit.DB.ReferenceArray':'Value.List',
        'Autodesk.Revit.DB.ReferenceArrayArray':'Value.List',
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
		'System.Collections.Generic.List{Autodesk.Revit.Creation.FamilyInstanceCreationData}':'List<Autodesk.Revit.Creation.FamilyInstanceCreationData>',
		'System.Collections.Generic.IList{Autodesk.Revit.DB.Curve}':'List<Autodesk.Revit.DB.Curve>',
		'System.Collections.Generic.IList{Autodesk.Revit.DB.Reference}':'List<Autodesk.Revit.DB.Reference>',
		'Autodesk.Revit.DB.Phase!System.Runtime.CompilerServices.IsByValue':'Autodesk.Revit.DB.Phase'
	}.get(x,x).replace('@','')

def conversion_method(x):
	return{
		'Autodesk.Revit.DB.ReferenceArrayArray':'dynRevitUtils.ConvertFSharpListListToReferenceArrayArray',
		'Autodesk.Revit.DB.ReferenceArray':'dynRevitUtils.ConvertFSharpListListToReferenceArray',
		'Autodesk.Revit.DB.CurveArrArray':'dynRevitUtils.ConvertFSharpListListToCurveArrayArray',
		'Autodesk.Revit.DB.CurveArray':'dynRevitUtils.ConvertFSharpListListToCurveArray',
		'System.Boolean':'Convert.ToBoolean'
	}.get(x,'')

if __name__ == "__main__":
    main()

