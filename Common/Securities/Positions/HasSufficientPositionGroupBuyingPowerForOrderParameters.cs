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

using System.Linq;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;

namespace QuantConnect.Securities.Positions
{
    /// <summary>
    /// Defines the parameters for <see cref="IPositionGroupBuyingPowerModel.HasSufficientBuyingPowerForOrder"/>
    /// </summary>
    public class HasSufficientPositionGroupBuyingPowerForOrderParameters
    {
        /// <summary>
        /// Gets the order
        /// </summary>
        public Order Order { get; }

        /// <summary>
        /// Gets the position group representing the holdings changes contemplated by the order
        /// </summary>
        public IPositionGroup PositionGroup { get; }

        /// <summary>
        /// Gets the algorithm's portfolio manager
        /// </summary>
        public SecurityPortfolioManager Portfolio { get; }

        /// <summary>
        /// Gets the algorithm's security manager
        /// </summary>
        public SecurityManager Securities { get; }

        /// <summary>
        /// Gets the algorithm's position group manager
        /// </summary>
        public PositionGroupManager PositionGroupManager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSufficientPositionGroupBuyingPowerForOrderParameters"/> class
        /// </summary>
        /// <param name="portfolio">The algorithm's portfolio manager</param>
        /// <param name="securities">The algorithm's security manager</param>
        /// <param name="positionGroupManager">The algorithm's position group manager</param>
        /// <param name="positionGroup">The position group</param>
        /// <param name="order">The order</param>
        public HasSufficientPositionGroupBuyingPowerForOrderParameters(
            SecurityPortfolioManager portfolio,
            SecurityManager securities,
            PositionGroupManager positionGroupManager,
            IPositionGroup positionGroup,
            Order order
            )
        {
            Order = order;
            Portfolio = portfolio;
            Securities = securities;
            PositionGroup = positionGroup;
            PositionGroupManager = positionGroupManager;
        }

        /// <summary>
        /// This may be called for non-combo type orders where the position group is guaranteed to have exactly one position
        /// </summary>
        public HasSufficientBuyingPowerForOrderParameters ToSufficientBuyingPowerForOrderParameters()
        {
            var position = PositionGroup.Single();
            var security = Securities[position.Symbol];
            return new HasSufficientBuyingPowerForOrderParameters(Portfolio, security, Order);
        }

        /// <summary>
        /// Implicit operator converting to the parameters for the impact helper function
        /// </summary>
        public static implicit operator ReservedBuyingPowerImpactParameters(
            HasSufficientPositionGroupBuyingPowerForOrderParameters parameters
            )
        {
            return new ReservedBuyingPowerImpactParameters(
                parameters.Securities,
                parameters.Portfolio,
                parameters.PositionGroupManager,
                parameters.PositionGroup
            );
        }
    }
}
