﻿using Dynamo.Interfaces;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Application-level handles provided to an extension when 
    /// Dynamo has started and is ready for interaction
    /// </summary>
    public class ReadyParams
    {
        private readonly DynamoModel dynamoModel;

        internal ReadyParams(DynamoModel dynamoM)
        {
            dynamoModel = dynamoM;
            dynamoModel.PropertyChanged += OnDynamoModelPropertyChanged;
        }

        public IEnumerable<IWorkspaceModel> WorkspaceModels
        {
            get
            {
                return dynamoModel.Workspaces;
            }
        }

        public IWorkspaceModel CurrentWorkspaceModel
        {
            get
            {
                return dynamoModel.CurrentWorkspace;
            }
        }

        public event Action<IWorkspaceModel> CurrentWorkspaceChanged;
        private void OnCurrentWorkspaceModelChanged(IWorkspaceModel ws)
        {
            if (CurrentWorkspaceChanged != null)
                CurrentWorkspaceChanged(ws);
        }

        private void OnDynamoModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorkspace")
                OnCurrentWorkspaceModelChanged((sender as DynamoModel).CurrentWorkspace);
        }
    }
}
