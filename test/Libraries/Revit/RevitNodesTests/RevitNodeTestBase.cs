﻿using NUnit.Framework;
using RevitServices.Transactions;

namespace DSRevitNodesTests
{
    /// <summary>
    /// Base class for units tests of Revit nodes.
    /// 
    /// </summary>
    public class RevitNodeTestBase
    {
        [SetUp]
        public virtual void Setup()
        {
            // create the transaction manager object
            TransactionManager.SetupManager(new AutomaticTransactionStrategy());

            // Tests do not run from idle thread.
            TransactionManager.Instance.DoAssertInIdleThread = false;
        }

        [TearDown]
        public virtual void TearDown()
        {
            // Automatic transaction strategy requires that we 
            // close the transaction if it hasn't been closed by 
            // by the end of an evaluation. It is possible to 
            // run the test framework without running Dynamo, so
            // we ensure that the transaction is closed here.
            TransactionManager.Instance.ForceCloseTransaction();
        }
    }
}
