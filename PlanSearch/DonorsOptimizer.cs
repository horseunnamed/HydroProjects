using System;
using System.Collections.Generic;
using LpSolveDotNet;

namespace PlanSearch
{
    internal class DonorsOptimizer
    {
        public static ISet<Donor> FindOptimalDonors(ISet<Donor> donors, CofinanceInfo bounds)
        {
            var result = new HashSet<Donor>();

            LpSolve.Init();

            using (var lp = LpSolve.make_lp(0, donors.Count))
            {
                lp.set_verbose(3);
                var boundRow = new double[donors.Count];
                var boundColno = new int[donors.Count];

                var targetRow = new double[donors.Count];
                var targetColno = new int[donors.Count];

                var i = 0;
                foreach (var donor in donors)
                {
                    lp.set_col_name(i + 1, donor.Channel.Id.ToString());
                    lp.set_binary(i + 1, true);
                    boundColno[i] = targetColno[i] = i + 1;
                    boundRow[i] = bounds.ChannelsPrices[donor.Channel.Id];
                    targetRow[i] = donor.Effect;
                    i++;
                }

                lp.set_add_rowmode(true);
                lp.add_constraintex(donors.Count, boundRow, boundColno, lpsolve_constr_types.LE, bounds.R);
                lp.set_add_rowmode(false);

                lp.set_obj_fnex(donors.Count, targetRow, targetColno);
                lp.set_maxim();

                var lpResult = lp.solve();

                if (lpResult != lpsolve_return.OPTIMAL)
                {
                    throw new Exception($"Optimization failed! LP result code: {lpResult}");
                }

                var variableValues = new double[donors.Count];
                lp.get_variables(variableValues);

                i = 0;
                foreach (var donor in donors)
                {
                    if (Math.Abs(variableValues[i] - 1) < 1e-10)
                    {
                        result.Add(donor);
                    }

                    i++;
                }
            }

            return result;
        }
    }
}
