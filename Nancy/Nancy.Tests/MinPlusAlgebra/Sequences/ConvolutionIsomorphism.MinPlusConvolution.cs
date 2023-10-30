using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public partial class ConvolutionIsomorphism
{
    public static IEnumerable<(Sequence f, Sequence g, Rational cutEnd, Rational cutCeiling, Rational T, Rational d)> IsospeedMinPlusConvolutions()
    {
        foreach (var (f, g) in Curves.ConvolutionIsomorphism.MinPlusConvolutionPairs())
        {
            var lcm_c = Rational.LeastCommonMultiple(f.PseudoPeriodHeight, g.PseudoPeriodHeight);
            var k_c_f = lcm_c / f.PseudoPeriodHeight;
            var k_c_g = lcm_c / g.PseudoPeriodHeight;
            var lcm_d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
            var d = Rational.Min(lcm_d, Rational.Max(k_c_f * f.PseudoPeriodLength, k_c_g * g.PseudoPeriodLength));
            var c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

            var tf = f.PseudoPeriodStart;
            var tg = g.PseudoPeriodStart;
            var T = tf + tg + lcm_d;
   
            var fSegmentAfterTf = f.GetSegmentAfter(tf);
            var tf_prime = (f.IsRightContinuousAt(tf) && fSegmentAfterTf.IsConstant) ? fSegmentAfterTf.EndTime : tf;
            var fCutEnd_minp = tf + 2 * lcm_d;
            var fCutEnd_iso = tf_prime + 2 * k_c_f * f.PseudoPeriodLength;
            var fCut = fCutEnd_minp <= fCutEnd_iso 
                ? f.Cut(tf, fCutEnd_minp, isEndIncluded: false)
                : f.Cut(tf, fCutEnd_iso, isEndIncluded: true);

            var gSegmentAfterTg = g.GetSegmentAfter(tg);
            var tg_prime = (g.IsRightContinuousAt(tg) && gSegmentAfterTg.IsConstant) ? gSegmentAfterTg.EndTime : tg;
            var gCutEnd_minp = tg + 2 * lcm_d;
            var gCutEnd_iso = tg_prime + 2 * k_c_g * g.PseudoPeriodLength;
            var gCut = gCutEnd_minp <= gCutEnd_iso 
                ? g.Cut(tg, gCutEnd_minp, isEndIncluded: false)
                : g.Cut(tg, gCutEnd_iso, isEndIncluded: true);

            var cutEnd = tf + tg + lcm_d + d;
            var cutCeiling = f.ValueAt(tf) + g.ValueAt(tg) + 2 * lcm_c;

            yield return (fCut, gCut, cutEnd, cutCeiling, T, d); 
        }
        
        var selectTestCases = new List<(Sequence f, Sequence g, Rational cutEnd, Rational cutCeiling, Rational T, Rational d)>
        {
            (
                f: new Sequence(new List<Element>{ new Point(new Rational(2370037, 15),96000), new Segment(new Rational(2370037, 15),179000,96000,0), new Point(179000,96000), new Segment(179000,new Rational(2685037, 15),96000,30000), new Point(new Rational(2685037, 15),170000), new Segment(new Rational(2685037, 15),200000,170000,0), new Point(200000,170000), new Segment(200000,new Rational(3000037, 15),170000,30000), new Point(new Rational(3000037, 15),244000), new Segment(new Rational(3000037, 15),221000,244000,0), new Point(221000,244000), new Segment(221000,new Rational(3315037, 15),244000,30000), new Point(new Rational(3315037, 15),318000), new Segment(new Rational(3315037, 15),242000,318000,0), new Point(242000,318000), new Segment(242000,new Rational(3630037, 15),318000,30000), new Point(new Rational(3630037, 15),392000), new Segment(new Rational(3630037, 15),263000,392000,0), new Point(263000,392000), new Segment(263000,new Rational(3945037, 15),392000,30000), new Point(new Rational(3945037, 15),466000), new Segment(new Rational(3945037, 15),284000,466000,0), new Point(284000,466000), new Segment(284000,new Rational(4260037, 15),466000,30000), new Point(new Rational(4260037, 15),540000), new Segment(new Rational(4260037, 15),305000,540000,0), new Point(305000,540000), new Segment(305000,new Rational(4575037, 15),540000,30000), new Point(new Rational(4575037, 15),614000), new Segment(new Rational(4575037, 15),326000,614000,0), new Point(326000,614000), new Segment(326000,new Rational(4890037, 15),614000,30000), new Point(new Rational(4890037, 15),688000), new Segment(new Rational(4890037, 15),347000,688000,0), new Point(347000,688000), new Segment(347000,new Rational(5205037, 15),688000,30000), new Point(new Rational(5205037, 15),762000), new Segment(new Rational(5205037, 15),368000,762000,0), new Point(368000,762000), new Segment(368000,new Rational(5520037, 15),762000,30000), new Point(new Rational(5520037, 15),836000), new Segment(new Rational(5520037, 15),389000,836000,0), new Point(389000,836000), new Segment(389000,new Rational(5835037, 15),836000,30000), new Point(new Rational(5835037, 15),910000), new Segment(new Rational(5835037, 15),410000,910000,0), new Point(410000,910000), new Segment(410000,new Rational(6150037, 15),910000,30000), new Point(new Rational(6150037, 15),984000), new Segment(new Rational(6150037, 15),431000,984000,0), new Point(431000,984000), new Segment(431000,new Rational(6465037, 15),984000,30000), new Point(new Rational(6465037, 15),1058000), new Segment(new Rational(6465037, 15),452000,1058000,0), new Point(452000,1058000), new Segment(452000,new Rational(6780037, 15),1058000,30000), new Point(new Rational(6780037, 15),1132000), new Segment(new Rational(6780037, 15),473000,1132000,0), new Point(473000,1132000), new Segment(473000,new Rational(7095037, 15),1132000,30000), new Point(new Rational(7095037, 15),1206000), new Segment(new Rational(7095037, 15),494000,1206000,0), new Point(494000,1206000), new Segment(494000,new Rational(7410037, 15),1206000,30000), new Point(new Rational(7410037, 15),1280000), new Segment(new Rational(7410037, 15),515000,1280000,0), new Point(515000,1280000), new Segment(515000,new Rational(7725037, 15),1280000,30000), new Point(new Rational(7725037, 15),1354000), new Segment(new Rational(7725037, 15),536000,1354000,0), new Point(536000,1354000), new Segment(536000,new Rational(8040037, 15),1354000,30000), new Point(new Rational(8040037, 15),1428000), new Segment(new Rational(8040037, 15),557000,1428000,0), new Point(557000,1428000), new Segment(557000,new Rational(8355037, 15),1428000,30000), new Point(new Rational(8355037, 15),1502000), new Segment(new Rational(8355037, 15),578000,1502000,0), new Point(578000,1502000), new Segment(578000,new Rational(8670037, 15),1502000,30000), new Point(new Rational(8670037, 15),1576000), new Segment(new Rational(8670037, 15),599000,1576000,0), new Point(599000,1576000), new Segment(599000,new Rational(8985037, 15),1576000,30000), new Point(new Rational(8985037, 15),1650000), new Segment(new Rational(8985037, 15),620000,1650000,0), new Point(620000,1650000), new Segment(620000,new Rational(9300037, 15),1650000,30000), new Point(new Rational(9300037, 15),1724000), new Segment(new Rational(9300037, 15),641000,1724000,0), new Point(641000,1724000), new Segment(641000,new Rational(9615037, 15),1724000,30000), new Point(new Rational(9615037, 15),1798000), new Segment(new Rational(9615037, 15),662000,1798000,0), new Point(662000,1798000), new Segment(662000,new Rational(9930037, 15),1798000,30000), new Point(new Rational(9930037, 15),1872000), new Segment(new Rational(9930037, 15),683000,1872000,0), new Point(683000,1872000), new Segment(683000,new Rational(10245037, 15),1872000,30000), new Point(new Rational(10245037, 15),1946000), new Segment(new Rational(10245037, 15),704000,1946000,0), new Point(704000,1946000), new Segment(704000,new Rational(10560037, 15),1946000,30000), new Point(new Rational(10560037, 15),2020000), new Segment(new Rational(10560037, 15),725000,2020000,0), new Point(725000,2020000), new Segment(725000,new Rational(10875037, 15),2020000,30000), new Point(new Rational(10875037, 15),2094000), new Segment(new Rational(10875037, 15),746000,2094000,0), new Point(746000,2094000), new Segment(746000,new Rational(11190037, 15),2094000,30000), new Point(new Rational(11190037, 15),2168000), new Segment(new Rational(11190037, 15),767000,2168000,0), new Point(767000,2168000), new Segment(767000,new Rational(11505037, 15),2168000,30000), new Point(new Rational(11505037, 15),2242000), new Segment(new Rational(11505037, 15),788000,2242000,0), new Point(788000,2242000), new Segment(788000,new Rational(11820037, 15),2242000,30000), new Point(new Rational(11820037, 15),2316000), new Segment(new Rational(11820037, 15),809000,2316000,0), new Point(809000,2316000), new Segment(809000,new Rational(12135037, 15),2316000,30000), new Point(new Rational(12135037, 15),2390000), new Segment(new Rational(12135037, 15),830000,2390000,0), new Point(830000,2390000), new Segment(830000,new Rational(12450037, 15),2390000,30000), new Point(new Rational(12450037, 15),2464000), new Segment(new Rational(12450037, 15),851000,2464000,0), new Point(851000,2464000), new Segment(851000,new Rational(12765037, 15),2464000,30000), new Point(new Rational(12765037, 15),2538000), new Segment(new Rational(12765037, 15),872000,2538000,0), new Point(872000,2538000), new Segment(872000,new Rational(13080037, 15),2538000,30000), new Point(new Rational(13080037, 15),2612000), new Segment(new Rational(13080037, 15),893000,2612000,0), new Point(893000,2612000), new Segment(893000,new Rational(13395037, 15),2612000,30000), new Point(new Rational(13395037, 15),2686000), new Segment(new Rational(13395037, 15),914000,2686000,0), new Point(914000,2686000), new Segment(914000,new Rational(13710037, 15),2686000,30000), new Point(new Rational(13710037, 15),2760000), new Segment(new Rational(13710037, 15),935000,2760000,0), new Point(935000,2760000), new Segment(935000,new Rational(14025037, 15),2760000,30000), new Point(new Rational(14025037, 15),2834000), new Segment(new Rational(14025037, 15),956000,2834000,0), new Point(956000,2834000), new Segment(956000,new Rational(14340037, 15),2834000,30000), new Point(new Rational(14340037, 15),2908000), new Segment(new Rational(14340037, 15),977000,2908000,0), new Point(977000,2908000), new Segment(977000,new Rational(14655037, 15),2908000,30000), new Point(new Rational(14655037, 15),2982000), new Segment(new Rational(14655037, 15),998000,2982000,0), new Point(998000,2982000), new Segment(998000,new Rational(14970037, 15),2982000,30000), new Point(new Rational(14970037, 15),3056000), new Segment(new Rational(14970037, 15),1019000,3056000,0), new Point(1019000,3056000), new Segment(1019000,new Rational(15285037, 15),3056000,30000), new Point(new Rational(15285037, 15),3130000), new Segment(new Rational(15285037, 15),1040000,3130000,0), new Point(1040000,3130000), new Segment(1040000,new Rational(15600037, 15),3130000,30000), new Point(new Rational(15600037, 15),3204000), new Segment(new Rational(15600037, 15),1061000,3204000,0), new Point(1061000,3204000), new Segment(1061000,new Rational(15915037, 15),3204000,30000), new Point(new Rational(15915037, 15),3278000), new Segment(new Rational(15915037, 15),1082000,3278000,0), new Point(1082000,3278000), new Segment(1082000,new Rational(16230037, 15),3278000,30000), new Point(new Rational(16230037, 15),3352000), new Segment(new Rational(16230037, 15),1103000,3352000,0), new Point(1103000,3352000), new Segment(1103000,new Rational(16545037, 15),3352000,30000), new Point(new Rational(16545037, 15),3426000), new Segment(new Rational(16545037, 15),1124000,3426000,0), new Point(1124000,3426000), new Segment(1124000,new Rational(16860037, 15),3426000,30000), new Point(new Rational(16860037, 15),3500000), new Segment(new Rational(16860037, 15),1145000,3500000,0), new Point(1145000,3500000), new Segment(1145000,new Rational(17175037, 15),3500000,30000), new Point(new Rational(17175037, 15),3574000), new Segment(new Rational(17175037, 15),1166000,3574000,0), new Point(1166000,3574000), new Segment(1166000,new Rational(17490037, 15),3574000,30000), new Point(new Rational(17490037, 15),3648000), new Segment(new Rational(17490037, 15),1187000,3648000,0), new Point(1187000,3648000) }),
                g: new Sequence(new List<Element>{ new Point(43000,48000), new Segment(43000,new Rational(559024, 13),48000,26000), new Point(new Rational(559024, 13),96000), new Segment(new Rational(559024, 13),86000,96000,0), new Point(86000,96000), new Segment(86000,new Rational(1118024, 13),96000,26000), new Point(new Rational(1118024, 13),144000), new Segment(new Rational(1118024, 13),129000,144000,0), new Point(129000,144000), new Segment(129000,new Rational(1677024, 13),144000,26000), new Point(new Rational(1677024, 13),192000), new Segment(new Rational(1677024, 13),172000,192000,0), new Point(172000,192000), new Segment(172000,new Rational(2236024, 13),192000,26000), new Point(new Rational(2236024, 13),240000), new Segment(new Rational(2236024, 13),215000,240000,0), new Point(215000,240000), new Segment(215000,new Rational(2795024, 13),240000,26000), new Point(new Rational(2795024, 13),288000), new Segment(new Rational(2795024, 13),258000,288000,0), new Point(258000,288000), new Segment(258000,new Rational(3354024, 13),288000,26000), new Point(new Rational(3354024, 13),336000), new Segment(new Rational(3354024, 13),301000,336000,0), new Point(301000,336000), new Segment(301000,new Rational(3913024, 13),336000,26000), new Point(new Rational(3913024, 13),384000), new Segment(new Rational(3913024, 13),344000,384000,0), new Point(344000,384000), new Segment(344000,new Rational(4472024, 13),384000,26000), new Point(new Rational(4472024, 13),432000), new Segment(new Rational(4472024, 13),387000,432000,0), new Point(387000,432000), new Segment(387000,new Rational(5031024, 13),432000,26000), new Point(new Rational(5031024, 13),480000), new Segment(new Rational(5031024, 13),430000,480000,0), new Point(430000,480000), new Segment(430000,new Rational(5590024, 13),480000,26000), new Point(new Rational(5590024, 13),528000), new Segment(new Rational(5590024, 13),473000,528000,0), new Point(473000,528000), new Segment(473000,new Rational(6149024, 13),528000,26000), new Point(new Rational(6149024, 13),576000), new Segment(new Rational(6149024, 13),516000,576000,0), new Point(516000,576000), new Segment(516000,new Rational(6708024, 13),576000,26000), new Point(new Rational(6708024, 13),624000), new Segment(new Rational(6708024, 13),559000,624000,0), new Point(559000,624000), new Segment(559000,new Rational(7267024, 13),624000,26000), new Point(new Rational(7267024, 13),672000), new Segment(new Rational(7267024, 13),602000,672000,0), new Point(602000,672000), new Segment(602000,new Rational(7826024, 13),672000,26000), new Point(new Rational(7826024, 13),720000), new Segment(new Rational(7826024, 13),645000,720000,0), new Point(645000,720000), new Segment(645000,new Rational(8385024, 13),720000,26000), new Point(new Rational(8385024, 13),768000), new Segment(new Rational(8385024, 13),688000,768000,0), new Point(688000,768000), new Segment(688000,new Rational(8944024, 13),768000,26000), new Point(new Rational(8944024, 13),816000), new Segment(new Rational(8944024, 13),731000,816000,0), new Point(731000,816000), new Segment(731000,new Rational(9503024, 13),816000,26000), new Point(new Rational(9503024, 13),864000), new Segment(new Rational(9503024, 13),774000,864000,0), new Point(774000,864000), new Segment(774000,new Rational(10062024, 13),864000,26000), new Point(new Rational(10062024, 13),912000), new Segment(new Rational(10062024, 13),817000,912000,0), new Point(817000,912000), new Segment(817000,new Rational(10621024, 13),912000,26000), new Point(new Rational(10621024, 13),960000), new Segment(new Rational(10621024, 13),860000,960000,0), new Point(860000,960000), new Segment(860000,new Rational(11180024, 13),960000,26000), new Point(new Rational(11180024, 13),1008000), new Segment(new Rational(11180024, 13),903000,1008000,0), new Point(903000,1008000), new Segment(903000,new Rational(11739024, 13),1008000,26000), new Point(new Rational(11739024, 13),1056000), new Segment(new Rational(11739024, 13),946000,1056000,0), new Point(946000,1056000), new Segment(946000,new Rational(12298024, 13),1056000,26000), new Point(new Rational(12298024, 13),1104000), new Segment(new Rational(12298024, 13),989000,1104000,0), new Point(989000,1104000), new Segment(989000,new Rational(12857024, 13),1104000,26000), new Point(new Rational(12857024, 13),1152000), new Segment(new Rational(12857024, 13),1032000,1152000,0), new Point(1032000,1152000), new Segment(1032000,new Rational(13416024, 13),1152000,26000), new Point(new Rational(13416024, 13),1200000), new Segment(new Rational(13416024, 13),1075000,1200000,0), new Point(1075000,1200000), new Segment(1075000,new Rational(13975024, 13),1200000,26000), new Point(new Rational(13975024, 13),1248000), new Segment(new Rational(13975024, 13),1118000,1248000,0), new Point(1118000,1248000), new Segment(1118000,new Rational(14534024, 13),1248000,26000), new Point(new Rational(14534024, 13),1296000), new Segment(new Rational(14534024, 13),1161000,1296000,0), new Point(1161000,1296000), new Segment(1161000,new Rational(15093024, 13),1296000,26000), new Point(new Rational(15093024, 13),1344000), new Segment(new Rational(15093024, 13),1204000,1344000,0), new Point(1204000,1344000), new Segment(1204000,new Rational(15652024, 13),1344000,26000), new Point(new Rational(15652024, 13),1392000), new Segment(new Rational(15652024, 13),1247000,1392000,0), new Point(1247000,1392000), new Segment(1247000,new Rational(16211024, 13),1392000,26000), new Point(new Rational(16211024, 13),1440000), new Segment(new Rational(16211024, 13),1290000,1440000,0), new Point(1290000,1440000), new Segment(1290000,new Rational(16770024, 13),1440000,26000), new Point(new Rational(16770024, 13),1488000), new Segment(new Rational(16770024, 13),1333000,1488000,0), new Point(1333000,1488000), new Segment(1333000,new Rational(17329024, 13),1488000,26000), new Point(new Rational(17329024, 13),1536000), new Segment(new Rational(17329024, 13),1376000,1536000,0), new Point(1376000,1536000), new Segment(1376000,new Rational(17888024, 13),1536000,26000), new Point(new Rational(17888024, 13),1584000), new Segment(new Rational(17888024, 13),1419000,1584000,0), new Point(1419000,1584000), new Segment(1419000,new Rational(18447024, 13),1584000,26000), new Point(new Rational(18447024, 13),1632000), new Segment(new Rational(18447024, 13),1462000,1632000,0), new Point(1462000,1632000), new Segment(1462000,new Rational(19006024, 13),1632000,26000), new Point(new Rational(19006024, 13),1680000), new Segment(new Rational(19006024, 13),1505000,1680000,0), new Point(1505000,1680000), new Segment(1505000,new Rational(19565024, 13),1680000,26000), new Point(new Rational(19565024, 13),1728000), new Segment(new Rational(19565024, 13),1548000,1728000,0), new Point(1548000,1728000), new Segment(1548000,new Rational(20124024, 13),1728000,26000), new Point(new Rational(20124024, 13),1776000), new Segment(new Rational(20124024, 13),1591000,1776000,0), new Point(1591000,1776000), new Segment(1591000,new Rational(20683024, 13),1776000,26000), new Point(new Rational(20683024, 13),1824000), new Segment(new Rational(20683024, 13),1634000,1824000,0), new Point(1634000,1824000), new Segment(1634000,new Rational(21242024, 13),1824000,26000), new Point(new Rational(21242024, 13),1872000), new Segment(new Rational(21242024, 13),1677000,1872000,0), new Point(1677000,1872000), new Segment(1677000,new Rational(21801024, 13),1872000,26000), new Point(new Rational(21801024, 13),1920000), new Segment(new Rational(21801024, 13),1720000,1920000,0), new Point(1720000,1920000), new Segment(1720000,new Rational(22360024, 13),1920000,26000), new Point(new Rational(22360024, 13),1968000), new Segment(new Rational(22360024, 13),1763000,1968000,0), new Point(1763000,1968000), new Segment(1763000,new Rational(22919024, 13),1968000,26000), new Point(new Rational(22919024, 13),2016000), new Segment(new Rational(22919024, 13),1806000,2016000,0), new Point(1806000,2016000), new Segment(1806000,new Rational(23478024, 13),2016000,26000), new Point(new Rational(23478024, 13),2064000), new Segment(new Rational(23478024, 13),1849000,2064000,0) }),
                cutEnd: new Rational(30105037, 15),
                cutCeiling: 3696000,
                T: new Rational(16560037, 15),
                d: 903000
            )
        };

        foreach (var selectTestCase in selectTestCases)
            yield return selectTestCase;
    }

    public static IEnumerable<object[]> IsospeedMinPlusConvolutionsTestCases()
    {
        foreach(var (f, g, cutEnd, cutCeiling, T, d) in IsospeedMinPlusConvolutions())
        {
            yield return new object[] { f, g, cutEnd, cutCeiling, T, d };
        }
    }

    [Theory]
    [MemberData(nameof(IsospeedMinPlusConvolutionsTestCases))]
    public void MinPlusConvolution_Isospeed_Equivalence(Sequence f, Sequence g, Rational cutEnd, Rational cutCeiling, Rational T, Rational d)
    {   
        // this test verifies that, for the cuts given by isospeed, we can compute the by-sequence convolution also via isomorphism

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        var settings = ComputationSettings.Default() with {
            UseBySequenceConvolutionIsomorphismOptimization = false
        };

        // direct algorithm, using min-plus convolution
        var direct_result = Sequence.Convolution(f, g, settings, cutEnd, cutCeiling, true, false, false);

        // inverse algorithm, using max-plus convolution
        var ta_f_prime = f.FirstPlateauEnd;
        var ta_g_prime = g.FirstPlateauEnd;

        var f_upi = f.UpperPseudoInverse(false);
        var g_upi = g.UpperPseudoInverse(false);
        var maxp = Sequence.MaxPlusConvolution(
            f_upi, g_upi,
            cutEnd: cutCeiling, cutCeiling: cutEnd,
            isEndIncluded: true, isCeilingIncluded: true,
            settings: settings);
        var inverse_raw = maxp.LowerPseudoInverse(false);

        // todo: rename in the actual alg as well
        Sequence inverse;
        if (ta_f_prime == f.DefinedFrom && ta_g_prime == g.DefinedFrom)
        {
            inverse = inverse_raw;
        }
        else
        {
            // note: does not handle left-open sequences
            var ext = Sequence.Constant(
                f.ValueAt(f.DefinedFrom) + g.ValueAt(g.DefinedFrom),
                f.DefinedFrom + g.DefinedFrom,
                ta_f_prime + ta_g_prime
            );
            inverse = Sequence.Minimum(ext, inverse_raw, false);
        }

        var inverse_result = inverse.Elements
            .CutWithCeiling(cutCeiling, false)
            .ToSequence();

        // results of the two methods, to be cut
        output.WriteLine($"var direct_result = {direct_result.ToCodeString()};");
        output.WriteLine($"var inverse_result = {inverse_result.ToCodeString()};");
        
        // cut of the two methods
        var direct_result_end = Rational.Min(
            direct_result.Elements.Last(e => e.IsFinite).EndTime,
            T + d
        );
        var direct_result_cut = direct_result.Cut(f.DefinedFrom + g.DefinedFrom, direct_result_end);
        var inverse_result_end = Rational.Min(
            inverse_result.Elements.Last(e => e.IsFinite).EndTime,
            T + d
        );
        var inverse_result_cut = inverse_result.Cut(f.DefinedFrom + g.DefinedFrom, inverse_result_end);
        output.WriteLine($"var direct_result_cut = {direct_result_cut.ToCodeString()};");
        output.WriteLine($"var inverse_result_cut = {inverse_result_cut.ToCodeString()};");
        
        Assert.True(Sequence.Equivalent(direct_result_cut, inverse_result_cut));
    }

    public static IEnumerable<(Sequence f, Sequence g)> LeftContinuousConvolutions()
    {
        var sequences = LeftContinuousExamples().Concat(ContinuousExamples());

        var pairs = sequences.SelectMany(
            f => sequences.Select(
                g => (f, g)
            )
        );
        return pairs;
    }

    public static IEnumerable<object[]> LeftContinuousConvolutionTestcases()
    {
        foreach (var (f, g) in LeftContinuousConvolutions())
        {
            yield return new object[] {f, g};
        }
    }

    [Theory]
    [MemberData(nameof(LeftContinuousConvolutionTestcases))]
    public void MinPlusConvolution_Generalization_Equivalence(Sequence f, Sequence g)
    {
        // this test verifies a conjecture that generalizes the isomorphism for by-sequence convolution
        // it states that the result of by-sequence convolution is 'valid', in the general case, only for the smaller of the lengths,
        // and within that the isomorphism can be applied
        
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        // for simplicity, we only support this case for now
        Assert.True(f.IsLeftClosed);
        Assert.True(g.IsLeftClosed);
        Assert.True(f.IsRightOpen);
        Assert.True(g.IsRightOpen);

        var ta_f = f.DefinedFrom;
        var ta_g = g.DefinedFrom;
        var tb_f = f.DefinedUntil;
        var tb_g = g.DefinedUntil;

        var lf = tb_f - ta_f;
        var lg = tb_g - ta_g;
        var length = Rational.Min(lf, lg);
        var cutStart = ta_f + ta_g;
        var cutEnd = ta_f + ta_g + length;

        var direct = Sequence.Convolution(f, g).Cut(cutStart, cutEnd);

        var ta_f_prime = f.FirstPlateauEnd;
        var ta_g_prime = g.FirstPlateauEnd;

        var f_upi = f.UpperPseudoInverse(false);
        var g_upi = g.UpperPseudoInverse(false);

        var maxp = Sequence.MaxPlusConvolution(f_upi, g_upi); //.Cut(vcutStart, vcutEnd, isEndIncluded: true);
        var inverse_raw = maxp.LowerPseudoInverse(false);

        output.WriteLine($"var direct = {direct.ToCodeString()};");
        output.WriteLine($"var inverse_raw = {inverse_raw.ToCodeString()};");

        if (ta_f_prime == ta_f && ta_g_prime == ta_g)
        {
            var inverse = inverse_raw.Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, inverse));
        }
        else
        {
            // todo: does not handle left-open sequences
            var ext = Sequence.Constant(
                f.ValueAt(ta_f) + g.ValueAt(ta_g), 
                ta_f + ta_g,
                ta_f_prime + ta_g_prime
            );
            var reconstructed = Sequence.Minimum(ext, inverse_raw, false)
                .Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, reconstructed));
        }
    }

    public static List<(Sequence a, Sequence b, Rational cutEnd)> SingleCutConvolutions()
    {
        var testcases = new List<(Sequence a, Sequence b, Rational cutEnd)>()
        {
            (
                new Sequence(new List<Element>{ new Point(2,1), new Segment(2,3,1,0), new Point(3,1), new Segment(3,4,1,1), new Point(4,2), new Segment(4,5,2,0), new Point(5,2), new Segment(5,6,2,1), new Point(6,3), new Segment(6,7,3,0), new Point(7,3), new Segment(7,8,3,1), new Point(8,4), new Segment(8,9,4,0), new Point(9,4), new Segment(9,10,4,1), new Point(10,5), new Segment(10,11,5,0), new Point(11,5), new Segment(11,12,5,1), new Point(12,6), new Segment(12,13,6,0), new Point(13,6), new Segment(13,14,6,1) }),
                new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,2,1), new Point(1,3), new Segment(1,3,3,0), new Point(3,3), new Segment(3,4,3,1), new Point(4,4), new Segment(4,6,4,0), new Point(6,4), new Segment(6,7,4,1), new Point(7,5), new Segment(7,9,5,0), new Point(9,5), new Segment(9,10,5,1), new Point(10,6), new Segment(10,12,6,0) }),
                14
            )
        };
        return testcases;
    }

    public static IEnumerable<object[]> SingleCutConvolutionTestcases()
    {
        foreach (var (a, b, cutEnd) in SingleCutConvolutions())
        {
            yield return new object[] {a, b, cutEnd};
        }
    }

    [Theory]
    [MemberData(nameof(SingleCutConvolutionTestcases))]
    public void MinPlusConvolution_SingleCut_Equivalence(Sequence f, Sequence g, Rational cutEnd)
    {
        // this test verifies a conjecture that generalizes the isomorphism for by-sequence convolution
        // it states that the result of by-sequence convolution is 'valid, in the general case, only for the smaller of the lengths,
        // and within that the isomorphism can be applied

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        var settings = ComputationSettings.Default() with {UseBySequenceConvolutionIsomorphismOptimization = false};

        // for simplicity, we only support this case for now
        Assert.True(f.IsLeftClosed);
        Assert.True(g.IsLeftClosed);
        Assert.True(f.IsRightOpen);
        Assert.True(g.IsRightOpen);

        var ta_f = f.DefinedFrom;
        var ta_g = g.DefinedFrom;
        var tb_f = f.DefinedUntil;
        var tb_g = g.DefinedUntil;

        var lf = tb_f - ta_f;
        var lg = tb_g - ta_g;
        var length = Rational.Min(lf, lg);
        var cutStart = ta_f + ta_g;
        var equivCutEnd = ta_f + ta_g + length;
        var cutCeiling = Rational.PlusInfinity;

        if (cutEnd > equivCutEnd)
            throw new InvalidOperationException();

        var direct = Sequence.Convolution(f, g, settings).Cut(cutStart, cutEnd);

        var ta_f_prime = f.FirstPlateauEnd;
        var ta_g_prime = g.FirstPlateauEnd;

        var f_upi = f.UpperPseudoInverse(false);
        var g_upi = g.UpperPseudoInverse(false);

        var maxp = Sequence.MaxPlusConvolution(
            f_upi, g_upi,
            cutEnd: cutCeiling, cutCeiling: cutEnd,
            isEndIncluded: true, isCeilingIncluded: true,
            settings: settings);
        var inverse_raw = maxp.LowerPseudoInverse(false);

        output.WriteLine($"var direct = {direct.ToCodeString()};");
        output.WriteLine($"var inverse_raw = {inverse_raw.ToCodeString()};");

        if (ta_f_prime == ta_f && ta_g_prime == ta_g)
        {
            var inverse = inverse_raw.Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, inverse));
        }
        else
        {
            // todo: does not handle left-open sequences
            var ext = Sequence.Constant(
                f.ValueAt(ta_f) + g.ValueAt(ta_g), 
                ta_f + ta_g,
                ta_f_prime + ta_g_prime
            );
            var reconstructed = Sequence.Minimum(ext, inverse_raw, false)
                .Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, reconstructed));
        }
    }
}