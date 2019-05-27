"""
Copyright 2018 Autodesk, Inc. All rights reserved.

This file is part of the Civil 3D Python Module.

"""
__author__ = 'Paolo Emilio Serra - paolo.serra@autodesk.com'
__copyright__ = '2018'
__version__ = '1.0.0'

__all__ = ['CivilEncoder', 'JPropertySetDefinition', 'JPropertyDefinition', 'civil_decoder']

import json
import pickle
import ast


class CivilEncoder(json.JSONEncoder):
    """A JSON encoder for PropertySets"""

    def default(self, obj):
        if isinstance(obj, (list, dict, str, unicode, int, float, long, bool, type(None))):
            return json.JSONEncoder.default(self, obj)
        if isinstance(obj, JPropertyDefinition):
            return {'Type': 'JPropertyDefinition',
                    'Data': {'Name': obj.Name,
                             'Description': obj.Description,
                             'DataType': obj.DataType,
                             'Automatic': obj.Automatic}
                    }
        if isinstance(obj, JPropertySetDefinition):
            return {'Type': 'JPropertySetDefinition',
                    'Data': {'Name': obj.Name,
                             'Description': obj.Description,
                             'AppliesToFilter': obj.AppliesToFilter,
                             'Definitions': [self.default(d) for d in obj.Definitions]}
                    }
        return {'civil3d': pickle.dumps(obj)}


class JPropertySetDefinition(object):
    """A class that represents a serializable property set definition."""

    def __init__(self, psdef=None):
        if psdef is not None:
            self.Name = psdef.Name
            self.Description = psdef.Description
            self.AppliesToFilter = [str(f) for f in psdef.AppliesToFilter]
            self.Definitions = [JPropertyDefinition(pdef) for pdef in psdef.Definitions]
        else:
            self.Definitions = []


class JPropertyDefinition(object):
    """A class that represents a serializable property definition."""

    def __init__(self, pdef=None):
        if pdef is not None:
            self.Name = pdef.Name
            self.Description = pdef.Description
            self.DataType = str(pdef.DataType)
            self.Automatic = pdef.Automatic
        else:
            pass


def civil_decoder(dct):
    """Returns an JSON representation for the property sets definitions."""
    output = None
    if 'Type' in dct:
        if dct['Type'] == 'JPropertySetDefinition':
            output = JPropertySetDefinition()
            data = dct['Data']
            output.Name = data['Name']
            output.Description = data['Description']
            output.AppliesToFilter = ast.literal_eval(str(data['AppliesToFilter']))
            output.Definitions = data['Definitions']
        elif dct['Type'] == 'JPropertyDefinition':
            output = JPropertyDefinition()
            data = dct['Data']
            output.Name = data['Name']
            output.Description = data['Description']
            output.DataType = data['DataType']
            output.Automatic = data['Automatic']
        return output
    return dct