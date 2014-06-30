﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;
using System.Linq;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an order and the items within it.
    /// </summary>
    public class TransactionActivity : MeasurementActivity
    {
        private readonly Dictionary<string, TransactionItemActivity> items = new Dictionary<string, TransactionItemActivity>();
        private decimal? orderTotal;

        /// <summary>
        /// Unique order ID for this order.
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Name of the store or the affiliation.
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Total of the items in the order prior to tax and shipping.
        /// </summary>
        public decimal OrderTotal
        {
            get { return orderTotal ?? Items.Sum(i => i.Price * i.Quantity); }
            set { orderTotal = value; }
        }

        /// <summary>
        /// Tax charge for this order.
        /// </summary>
        public decimal TaxCost { get; set; }

        /// <summary>
        /// Shipping charge for this order.
        /// </summary>
        public decimal ShippingCost { get; set; }

        /// <summary>
        /// Final total for this order including items, shipping and tax.
        /// </summary>
        public decimal FinalTotal
        {
            get { return OrderTotal + TaxCost + ShippingCost; }
        }

        /// <summary>
        /// Currency of the order.
        /// </summary>
        /// <example>USD</example>
        public string Currency { get; set; }

        /// <summary>
        /// Add an itemActivity to the transaction.
        /// </summary>
        /// <param name="itemActivity"></param>
        public void AddItem(TransactionItemActivity itemActivity)
        {
            // We use a dictionary to simulate the correct behavior - each SKU can only have one entry and last wins.
            items[itemActivity.Code] = itemActivity;
        }

        /// <summary>
        /// Clear all the items from a transaction.
        /// </summary>
        public void ClearItems()
        {
            items.Clear();
            orderTotal = null;
        }

        /// <summary>
        /// Enumeration of the Items that form the transaction.
        /// </summary>
        public IEnumerable<TransactionItemActivity> Items
        {
            get { return items.Values.AsEnumerable(); }
        }
    }
}