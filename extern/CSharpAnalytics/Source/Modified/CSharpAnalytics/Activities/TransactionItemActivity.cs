﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an item on an order.
    /// </summary>
    public class TransactionItemActivity : MeasurementActivity
    {
        private readonly string code;
        private readonly string name;
        private readonly decimal price;
        private readonly int quantity;
        private readonly string variation;

        /// <summary>
        /// Product code or SKU.
        /// </summary>
        /// <example>DD44</example>
        public string Code
        {
            get { return code; }
        }

        /// <summary>
        /// Product name.
        /// </summary>
        /// <example>T-Shirt</example>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Unit price.
        /// </summary>
        /// <example>11.99</example>
        public decimal Price
        {
            get { return price; }
        }

        /// <summary>
        /// Quantity.
        /// </summary>
        /// <example>1</example>
        public int Quantity
        {
            get { return quantity; }
        }

        /// <summary>
        /// Optional variation or category.
        /// </summary>
        /// <example>Olive Medium</example>
        public string Variation
        {
            get { return variation; }
        }

        /// <summary>
        /// Transaction this item is associated with.
        /// </summary>
        /// <remarks>
        /// This is automatically set by analytics to the previous transaction sent.
        /// </remarks>
        internal TransactionActivity Transaction { get; set; }

        /// <summary>
        /// Create a new transaction item capturing necessary parameters.
        /// </summary>
        /// <param name="code">Product code or SKU.</param>
        /// <param name="name">Product name.</param>
        /// <param name="price">Unit price.</param>
        /// <param name="quantity">Quantity.</param>
        /// <param name="variation">Variation or category.</param>
        public TransactionItemActivity(string code, string name, decimal price, int quantity, string variation = null)
        {
            this.code = code;
            this.name = name;
            this.price = price;
            this.quantity = quantity;
            this.variation = variation;
        }
    }

    public static class TransactionItemExtensions
    {
        /// <summary>
        /// Add an item to a transaction.
        /// </summary>
        /// <param name="transaction">Transaction to add an item to.</param>
        /// <param name="code">Product code or SKU.</param>
        /// <param name="name">Product name.</param>
        /// <param name="price">Unit price.</param>
        /// <param name="quantity">Quantity.</param>
        /// <param name="variation">Variation or category.</param>
        public static void AddItem(this TransactionActivity transaction, string code, string name, decimal price, int quantity, string variation = null)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            transaction.AddItem(new TransactionItemActivity(code, name, price, quantity, variation));
        }
    }
}