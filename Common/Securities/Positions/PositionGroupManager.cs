/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using QuantConnect.Interfaces;

namespace QuantConnect.Securities.Positions
{
    /// <summary>
    /// Manages an <see cref="IAlgorithm"/>'s collection of <see cref="IPositionGroup"/>
    /// </summary>
    public class PositionGroupManager : IReadOnlyCollection<IPositionGroup>
    {
        /// <summary>
        /// Gets the count of position groups under management
        /// </summary>
        public int Count => Groups?.Count ?? 0;

        private bool _requiresGroupResolution;
        private readonly SecurityManager _securities;
        private readonly CompositePositionGroupResolver _resolver;
        private readonly HashSet<IPositionGroupDescriptor> _descriptors;

        /// <summary>
        /// Gets the resolver used to determine the set of <see cref="IPositionGroup"/> for the algorithm
        /// </summary>
        public IPositionGroupResolver Resolver => _resolver;

        /// <summary>
        /// Gets the current set of <see cref="IPositionGroup"/> as resolved by the <see cref="Resolver"/>.
        /// NOTE: Do not save a reference to this property as it is changed each time groups are re-resolved
        /// </summary>
        public PositionGroupCollection Groups { get; private set; }

        /// <summary>
        /// Gets the collection of <see cref="IPositionGroupDescriptor"/> defining how groups are resolved and constructed
        /// </summary>
        public IReadOnlyCollection<IPositionGroupDescriptor> Descriptors => _descriptors;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionGroupManager"/> class
        /// </summary>
        /// <param name="securities">The algorithm's security manager</param>
        public PositionGroupManager(SecurityManager securities)
        {
            _securities = securities;
            Groups = PositionGroupCollection.Empty;
            _resolver = new CompositePositionGroupResolver(SecurityPositionGroupResolver.Instance);
            _descriptors = new HashSet<IPositionGroupDescriptor> {SecurityPositionGroupDescriptor.Instance};

            // we must be notified each time our holdings change, so each time a security is added, we
            // want to bind to its SecurityHolding.QuantityChanged event so we can trigger the resolver

            securities.CollectionChanged += (sender, args) =>
            {
                foreach (Security security in args.NewItems)
                {
                    SecurityPositionGroup group;
                    if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        if (!Groups.TryGetSecurityGroup(security.Symbol, out group))
                        {
                            // simply adding a security doesn't require group resolution until it has holdings
                            // all we need to do is make sure we add the default SecurityPositionGroup
                            Groups = Groups.SetItem(new SecurityPositionGroup(security));
                            security.Holdings.QuantityChanged += HoldingsOnQuantityChanged;
                            if (security.Invested)
                            {
                                // if this security has holdings then we'll need to resolve position groups
                                _requiresGroupResolution = true;
                            }
                        }
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Remove)
                    {
                        if (Groups.TryGetSecurityGroup(security.Symbol, out group))
                        {
                            security.Holdings.QuantityChanged -= HoldingsOnQuantityChanged;
                            if (security.Invested)
                            {
                                // only trigger group resolution if we had holdings in the removed security
                                _requiresGroupResolution = true;
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Registers the specified <paramref name="descriptor"/> and sets at which index its resolver should run.
        /// Specify <code>index=0</code> to run first and <code>index=Descriptors.Count-1</code> to run last. The
        /// last resolver is always the default resolver for the <see cref="SecurityPositionGroup"/>.
        /// </summary>
        /// <param name="descriptor">The position group's descriptor to register</param>
        /// <param name="index">The index the descriptor's resolver should run at.</param>
        public void RegisterDescriptor(IPositionGroupDescriptor descriptor, int index)
        {
            if (index >= Descriptors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index,
                    "Index must be less than the Descriptors.Count to ensure the SecurityPositionGroupResolver always runs last."
                );
            }

            if (_descriptors.Add(descriptor))
            {
                _resolver.Add(descriptor.Resolver, index);
            }
        }

        /// <summary>
        /// Re-resolves the current set of <see cref="IPositionGroup"/> for the algorithm.
        /// </summary>
        public void ResolvePositionGroups()
        {
            if (_requiresGroupResolution)
            {
                Groups = Resolver.ResolvePositionGroups(
                    PositionCollection.Create(_securities.Values)
                );
            }
        }

        /// <summary>
        /// Gets the algorithm's current holdings in the position group identified by the specified <paramref name="key"/>
        /// </summary>
        /// <param name="key">The key defining the position group we seek</param>
        /// <returns>An <see cref="IPositionGroup"/> containing the algorithm's current holdings in this specified group</returns>
        public IPositionGroup GetPositionGroup(PositionGroupKey key)
        {
            IPositionGroup group;
            return Groups.TryGetPositionGroup(key, out group) ? group : PositionGroup.Empty(key);
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IPositionGroup> GetEnumerator()
        {
            return Groups.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void HoldingsOnQuantityChanged(object sender, SecurityHoldingQuantityChangedEventArgs e)
        {
            // we don't want to re-resolve every time quantity changes because then we might by re-resolving
            // multiple times in the same time step. instead, we'll mark us as needing resolution and the
            // AlgorithmManager will invoke ResolvePositionGroups at the end of the time step
            _requiresGroupResolution = true;
        }
    }
}
