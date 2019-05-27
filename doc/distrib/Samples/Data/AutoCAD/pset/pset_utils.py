"""
Copyright 2018 Autodesk, Inc. All rights reserved.

This file is part of the Civil 3D Python Module.

"""
__author__ = 'Paolo Emilio Serra - paolo.serra@autodesk.com'
__copyright__ = '2018'
__version__ = '1.0.0'

import clr

# Add Assemblies for AutoCAD and Civil 3D APIs
clr.AddReference('acmgd')
clr.AddReference('acdbmgd')
clr.AddReference('accoremgd')
clr.AddReference('AecBaseMgd')
clr.AddReference('AecPropDataMgd')
clr.AddReference('AeccDbMgd')
clr.AddReference('AeccPressurePipesMgd')
clr.AddReference('acdbmgdbrep')
clr.AddReference('System.Windows.Forms')

# Add standard Python references
import sys
sys.path.append('C:\Program Files (x86)\IronPython 2.7\Lib')
import os
import csv
import json

# Add references to manage arrays, collections and interact with the user
from System import *
from System.IO import *
from System.Collections.Specialized import *
from System.Windows.Forms import MessageBox

# Create an alias to the Autodesk.AutoCAD.ApplicationServices.Application class
import Autodesk.AutoCAD.ApplicationServices.Application as acapp

# Import references from AutoCAD
from Autodesk.AutoCAD.Runtime import *
from Autodesk.AutoCAD.ApplicationServices import *
from Autodesk.AutoCAD.EditorInput import *
from Autodesk.AutoCAD.DatabaseServices import *
from Autodesk.AutoCAD.Geometry import *

# Import references for PropertySets
from Autodesk.Aec.PropertyData import *
from Autodesk.Aec.PropertyData.DatabaseServices import *

# Import references for Civil 3D
from Autodesk.Civil.ApplicationServices import *
from Autodesk.Civil.DatabaseServices import *


# Create reference to the serialization module
from pset.pset_serialization import *

__all__ = ['create_ps_definitions_json', 'update_ps_values_csv', 'dump_ps_definitions_json', 'dump_ps_values_csv']

adoc = acapp.DocumentManager.MdiActiveDocument
ed = adoc.Editor


def create_ps_definitions_json(jsonpath):
    """Creates a property set by JSON input.
    :param jsonpath: The fully qualified path to the JSON file that contains the Property sets definitions
    :returns:
    """
    global adoc

    with adoc.LockDocument():
        with adoc.Database as db:
            with open(jsonpath, 'rb') as f:
                psdef = None
                psdefid = None
                newps = False
                with db.TransactionManager.StartTransaction() as t:
                    dpsd = DictionaryPropertySetDefinitions(db)
                    try:
                        jpsd = json.load(f, object_hook=civil_decoder)
                        name = jpsd.Name
                        filter = jpsd.AppliesToFilter
                        if not dpsd.Has(name, t):
                            psdef = PropertySetDefinition()
                            psdefid = psdef.Id
                            dpsd.AddNewRecord(name, psdef)
                            newps = True

                        else:
                            psdefid = dpsd.GetAt(name)
                            psdef = t.GetObject(psdefid, OpenMode.ForWrite)

                        psdef.AppliesToAll = False
                        sc = StringCollection()
                        sc.AddRange(Array[str](filter))
                        psdef.SetAppliesToFilter(sc, False)
                        psdef.Description = jpsd.Description
                        definitions = psdef.Definitions

                        for jpd in jpsd.Definitions:
                            pd = PropertyDefinition()
                            pd.Automatic = jpd.Automatic
                            pd.DataType = DataType.Parse(DataType, jpd.DataType, True)
                            pd.Description = jpd.Description
                            pd.Name = jpd.Name                     
                            if not definitions.Contains(pd):
                                definitions.Add(pd)
                        if newps:
                            t.AddNewlyCreatedDBObject(psdef, True)
                    except Exception as ex:
                        MessageBox.Show('Create Property Set\r\n{}'.format(ex.message))

                    t.Commit()


    MessageBox.Show('{} Property set created.'.format(name))


def update_ps_values_csv(csvpath, name):
    """Selects an object by handle and assigns the specified property set values.

    csvpath: the full name of the csv file that contains the property values.
    name: the name of the property set.
    """
    global adoc
    global ed
    db = adoc.Database
    if csvpath is None or name is None:
        return

    with adoc.LockDocument():
        with adoc.Database as db:
            dpsd = DictionaryPropertySetDefinitions(db)
            psdef = None
            # Check if the property set exists
            try:
                psdef = dpsd.GetAt(name)
            except Exception as ex:
                MessageBox.Show('Property Set\r\n{}'.format(ex.message))
            if psdef is None:
                return  # Fails gracefully with no property set with that name
            # Safely manage the Database
            with db.TransactionManager.StartTransaction() as t:
                # Read the source file
                with open(csvpath, 'rb') as f:
                    reader = [r for r in csv.reader(f, dialect='excel')]
                    headers = []
                    hi = 0
                    for i, row in enumerate(reader):
                        # Get the headers from the first row with the property names
                        if i == 0:
                            headers = row
                            hi = headers.index('Handle')
                            continue

                        # Process the remaining rows
                        # Get an object by handle
                        hndl = None
                        try:
                            hndl = Handle(Convert.ToInt64(row[hi], 16))
                        except Exception as ex:
                            MessageBox.Show('Handle\n{}\n[}'.format(row, ex.message))
                            return
                        id = None
                        try:
                            id = db.GetObjectId(False, hndl, 0)
                        except Exception as ex:
                            MessageBox.Show('{}'.format(ex.message))
                        if id is None:
                            continue
                        obj = t.GetObject(id, OpenMode.ForWrite)

                        # Check if the object has the psdef associated
                        obj_ps = None
                        try:
                            PropertyDataServices.AddPropertySet(obj, psdef.ObjectId)
                        except Exception as ex:
                            MessageBox.Show('Assign Property Set\r\n{0}\r\n{1}'.format(hndl, ex.message))
                            return
                        for psid in PropertyDataServices.GetPropertySets(obj):
                            obj_ps = t.GetObject(psid, OpenMode.ForWrite)
                            if obj_ps.PropertySetDefinition == psdef.ObjectId:
                                break

                        # Assign the values to the properties in the row
                        for j, value in enumerate(row):
                            try:
                                if j != hi:
                                    pid = obj_ps.PropertyNameToId(headers[j])
                                    prop = [p for p in obj_ps.PropertySetData if p.Id == pid]
                                    if len(prop) > 0:
                                        prop = prop[0]
                                    else:
                                        continue
                                    if prop.DataType == DataType.Real:
                                        value = float(value)
                                    elif prop.DataType == DataType.Integer:
                                        value = int(value)
                                    elif prop.DataType == DataType.TrueFalse:
                                        value = bool(value)
                                    elif prop.DataType == DataType.Text:
                                        value = str(value)
                                    elif prop.DataType == DataType.AlphaIncrement:
                                        pass
                                    elif prop.DataType == DataType.AutoIncrement:
                                        pass
                                    elif prop.DataType == DataType.Graphic:
                                        pass
                                    elif prop.DataType == DataType.List:
                                        pass
                                    else:
                                        pass
                                    obj_ps.SetAt(pid, value)
                            except Exception as ex:
                                MessageBox.Show('Assign Property Value\r\n{0}\n{1}'.format(hndl, headers[j]))
                                continue
                t.Commit()
    MessageBox.Show('Update Property Set Completed.')


def dump_ps_definitions_json(folderpath, name=None):
    """Creates a JSON representation of a property set definition.

    folderpath: the full path of the folder that will contain the JSON dump.
    name: the name of the property set.
    """
    global adoc

    with adoc.LockDocument():
        with adoc.Database as db:
            with db.TransactionManager.StartTransaction() as t:
                psdefidcoll = []
                dpsd = DictionaryPropertySetDefinitions(db)
                if name != '' and name is not None:
                    try:
                        psdefidcoll.append(dpsd.GetAt(name))
                    except Exception as ex:
                        MessageBox.Show('Property Set\r\n{}'.format(ex.message))
                else:
                    psdefidcoll = dpsd.Records

                for psdefid in psdefidcoll:
                    psdef = None
                    if psdefid is None:
                        continue  # Skips the None property set
                    psdef = t.GetObject(psdefid, OpenMode.ForRead)

                    jspath = os.path.join(folderpath, psdef.Name + '.json')

                    with open(jspath, 'wb') as f:
                        try:
                            output = ''
                            output = json.dumps(JPropertySetDefinition(psdef), indent=4, cls=CivilEncoder,
                                                ensure_ascii=False)
                            output.replace('True', 'true').replace('False', 'false').replace('None', 'null')
                            f.write(output)
                        except:
                            pass
    MessageBox.Show('Dump Property Set to JSON Completed.')


def dump_ps_values_csv(folderpath, name=None):
    """Dumps a CSV file containing the property set values for the objects in the model.

    folderpath: the full path of the folder that will contain the CSV file.
    name: the property set name, if None it exports all property sets in the document.
    """
    global adoc

    with adoc.LockDocument():
        with adoc.Database as db:
            with db.TransactionManager.StartTransaction() as t:

                dpsd = DictionaryPropertySetDefinitions(db)
                psdefidcoll = []
                if name != '' and name is not None:
                    try:
                        psdefidcoll.append(dpsd.GetAt(name))
                    except Exception as ex:
                        MessageBox.Show('Property Set\r\n{}'.format(ex.message))
                else:
                    psdefidcoll = dpsd.Records

                for psdefid in psdefidcoll:
                    psdef = None
                    if psdefid is None:
                        continue  # Skips the None property set
                    psdef = t.GetObject(psdefid, OpenMode.ForRead)
                    csvpath = os.path.join(folderpath, 'PropertySet_{}.csv'.format(psdef.Name))

                    with open(csvpath, 'wb') as f:
                        writer = csv.writer(f, quotechar='"', quoting=csv.QUOTE_ALL)
                        headers = ['Handle']
                        for pd in psdef.Definitions:
                            if pd.Name not in headers:
                                headers.append(pd.Name)
                        writer.writerow(headers)
                        for oid in PropertyDataServices.GetAllPropertySetsUsingDefinition(psdefid, False):
                            ps = t.GetObject(oid, OpenMode.ForRead)
                            temp = []
                            for h in headers:
                                if h == 'Handle':
                                    temp.append(str(ps.ObjectAttachedTo.Handle))
                                else:
                                    try:
                                        a = ps.PropertyNameToId(h)
                                        b = ps.GetAt(a)
                                        temp.append('{}'.format(b))
                                    except Exception() as ex:
                                        temp.append(ex.message)
                            writer.writerow(temp)

    MessageBox.Show('Dump Property Set to CSV Completed.')
