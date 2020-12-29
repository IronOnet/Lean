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

using System.Collections.Generic;

namespace QuantConnect.Securities.Positions
{
    /// <summary>
    /// Parameters for the <see cref="IPositionGroupBuyingPowerModel.GetReservedBuyingPowerImpact"/>
    /// </summary>
    public class ReservedBuyingPowerImpactParameters
    {
        /// <summary>
        /// Gets the algorithm's security manager
        /// </summary>
        public SecurityManager Securities { get; }

        /// <summary>
        /// Gets the position changes being contemplated
        /// </summary>
        public IReadOnlyCollection<IPosition> ContemplatedChanges { get; }

        /// <summary>
        /// Gets the algorithm's portfolio manager
        /// </summary>
        public SecurityPortfolioManager Portfolio { get; }

        /// <summary>
        /// Gets the algorithm's position group manager
        /// </summary>
        public PositionGroupManager PositionGroupManager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservedBuyingPowerImpactParameters"/> class
        /// </summary>
        /// <param name="securities">The algorithm's securities manager</param>
        /// <param name="portfolio">The algorithm's portfolio manager</param>
        /// <param name="positionGroupManager">The algorithm's position group manager</param>
        /// <param name="contemplatedChanges">The position changes being contemplated</param>
        public ReservedBuyingPowerImpactParameters(
            SecurityManager securities,
            SecurityPortfolioManager portfolio,
            PositionGroupManager positionGroupManager,
            IReadOnlyCollection<IPosition> contemplatedChanges
            )
        {
            Portfolio = portfolio;
            Securities = securities;
            ContemplatedChanges = contemplatedChanges;
            PositionGroupManager = positionGroupManager;
        }
    }
}
