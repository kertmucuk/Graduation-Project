using BP_WebSite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using System.Web.Security;
using System.Windows.Markup;

namespace BP_WebSite.Controllers
{
    public class HomeController : Controller 
    {
        BP_DatabaseEntities BP_DatabaseEntities = new BP_DatabaseEntities();
        public static Kullanici currentuser;

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }


        [AllowAnonymous]
        public ActionResult Login(Kullanici kullanici, string sifre)
        {
            List<Kullanici> kullanicilar = BP_DatabaseEntities.Kullanici.ToList();
            var user = kullanicilar.FirstOrDefault(x => x.kullaniciAdi == kullanici.kullaniciAdi && x.sifre == sifre);
            if (user != null)
            {
                kullanici.kullaniciId = user.kullaniciId;
                kullanici.sifre = sifre;
                currentuser = kullanici;
                FormsAuthentication.SetAuthCookie(kullanici.kullaniciAdi, false);
                return RedirectToAction("Index", "Home");
            }
            else if (kullanici.kullaniciAdi != null)
                ViewBag.LoginError = "Hatalı kullanici adı veya şifre.";
            return View();
        }

        [AllowAnonymous]
        public ActionResult Register(string kullaniciadi, string sifre1, string sifre2)
        {
            List<Kullanici> kullanicilar = BP_DatabaseEntities.Kullanici.ToList();
            var user = kullanicilar.FirstOrDefault(x => x.kullaniciAdi == kullaniciadi);
            if (sifre1 != null && sifre2 != null)
            {
                if (user == null && sifre1.Equals(sifre2))
                {
                    Kullanici kullanici = new Kullanici();
                    kullanici.kullaniciAdi = kullaniciadi;
                    kullanici.sifre = sifre1;
                    BP_DatabaseEntities.Kullanici.Add(kullanici);
                    BP_DatabaseEntities.SaveChanges();
                    kullanicilar = BP_DatabaseEntities.Kullanici.ToList();
                    user = kullanicilar.FirstOrDefault(x => x.kullaniciAdi == kullaniciadi && x.sifre == sifre1);
                    currentuser = user;
                    FormsAuthentication.SetAuthCookie(kullanici.kullaniciAdi, false);
                    return RedirectToAction("Index", "Home");
                }
                else if (user != null)
                {
                    ViewBag.LoginError = "Bu kullanıcı adı kullanılıyor.";
                    return View();
                }
                else if (!sifre1.Equals(sifre2))
                {
                    ViewBag.LoginError = "Parolalar eşleşmeli, tekrar deneyiniz!.";
                    return View();
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult Logout()
        {
            currentuser = null;
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }

        public ActionResult Eleme()
        {
            return View();
        }
        [Authorize]
        public ActionResult Lig()
        {
            return View();
        }

        [Authorize]
        public ActionResult Turnuvalar()
        {
            List<Turnuva> turnuvaList = new List<Turnuva>();
            var groups = BP_DatabaseEntities.Turnuva.GroupBy(p => p.kullaniciId);
            foreach (var group in groups)
                if (group.Key.Value == currentuser.kullaniciId)
                    foreach (var turnuva in group)
                        turnuvaList.Add(turnuva);
            return View(turnuvaList);
        }

        public int id;
        [Authorize]
        public ActionResult Yonet(int id)
        {
            Turnuva t = BP_DatabaseEntities.Turnuva.FirstOrDefault(x => x.turnuvaId == id);
            BP_DatabaseEntities.Entry(t).Collection(x => x.Round).Load();
            ICollection<Round> rounds = t.Round;
            List<Round> round_list = new List<Round>();
            foreach (Round r in rounds)
                round_list.Add(r);
            ViewBag.turnuvaadi = t.turnuvaAdi;
            ViewBag.turnuvaid = id;
            ViewBag.turnuva_cesidi = t.turnuvaCesidi;
            return View(round_list);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Kaydet_Lig(List<string> values, int id)
        {
            for (int i = 0; i < values.Count; i += 3)
            {
                try
                {
                    if (BP_DatabaseEntities.Turnuva.Find(id).turnuvaCesidi == "lig")
                    {
                        if (BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor1 != null && BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor2 != null)
                        {
                            if (BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor1 > BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor2)
                            {
                                BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID1).pts -= 3;
                            }
                            else if (BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor1 < BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor2)
                            {
                                BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID2).pts -= 3;
                            }
                            else
                            {
                                BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID1).pts -= 1;
                                BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID2).pts -= 1;
                            }
                            BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID1).avg -= Convert.ToInt32(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor1) - Convert.ToInt32(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor2);
                            BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID2).avg -= Convert.ToInt32(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor2) - Convert.ToInt32(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor1);
                        }

                        BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor1 = Int16.Parse(values[i]);
                        BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).skor2 = Int16.Parse(values[i + 1]);
                        if (Int16.Parse(values[i]) > Int16.Parse(values[i + 1]))
                        {
                            BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID1).pts += 3;
                        }
                        else if (Int16.Parse(values[i]) < Int16.Parse(values[i + 1]))
                        {
                            BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID2).pts += 3;
                        }
                        else
                        {
                            BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID1).pts += 1;
                            BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID2).pts += 1;
                        }
                        BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID1).avg += Int16.Parse(values[i]) - Int16.Parse(values[i + 1]);
                        BP_DatabaseEntities.Takim.Find(BP_DatabaseEntities.Match.Find(Int16.Parse(values[i + 2])).takimID2).avg += Int16.Parse(values[i + 1]) - Int16.Parse(values[i]);
                    }
                }
                catch { }
            }

            BP_DatabaseEntities.SaveChanges();
            return RedirectToAction("Yonet", "Home", new { id = id });
        }

        [Authorize]
        [HttpPost]
        public ActionResult Kaydet_Eleme(List<string> values, int id)
        {
            int k = 0;
            Turnuva t = BP_DatabaseEntities.Turnuva.FirstOrDefault(x => x.turnuvaId == id);
            BP_DatabaseEntities.Entry(t).Collection(x => x.Round).Load();
            ICollection<Round> rounds = t.Round;
            int temp_round_id = rounds.ElementAt(0).roundId;
            foreach (Round r in rounds)
            {
                ICollection<Match> match = r.Match;
                List<Match> matches = new List<Match>();
                matches.Clear();
                foreach (Match m in match)
                    matches.Add(m);

                int temp_match = 0;

                for (int j = 0; j < matches.Count; j += 2)
                {

                    try
                    {
                        if (values[k] != values[k + 1] && values[k] != "" && values[k + 1] != "")
                        {
                            BP_DatabaseEntities.Match.Find(matches[j].matchId).skor1 = Convert.ToInt32(values[k]);
                            BP_DatabaseEntities.Match.Find(matches[j].matchId).skor2 = Convert.ToInt32(values[k + 1]);
                            BP_DatabaseEntities.SaveChanges();
                        }

                        if (values[k + 2] != values[k + 3] && values[k + 2] != "" && values[k + 3] != "")
                        {
                            BP_DatabaseEntities.Match.Find(matches[j + 1].matchId).skor1 = Convert.ToInt32(values[k + 2]);
                            BP_DatabaseEntities.Match.Find(matches[j + 1].matchId).skor2 = Convert.ToInt32(values[k + 3]);
                            BP_DatabaseEntities.SaveChanges();
                        }
                    }
                    catch
                    {
                    }
                    if (matches.Count != 1)
                    {
                        if (matches[j].takimID1 != null && matches[j].takimID2 != null && matches[j].skor1 != null && matches[j].skor2 != null)
                        {
                            if (BP_DatabaseEntities.Match.Find(matches[j].matchId).skor1 > BP_DatabaseEntities.Match.Find(matches[j].matchId).skor2)
                            {
                                BP_DatabaseEntities.Match.Find(matches[j].matchId + match.Count - temp_match).takimID1
                                    = BP_DatabaseEntities.Match.Find(matches[j].matchId).takimID1;
                            }
                            else
                            {
                                BP_DatabaseEntities.Match.Find(matches[j].matchId + match.Count - temp_match).takimID1
                                    = BP_DatabaseEntities.Match.Find(matches[j].matchId).takimID2;
                            }
                            BP_DatabaseEntities.SaveChanges();
                        }
                        else if (matches[j].takimID1 != null && matches[j].takimID2 == null && r.roundId == temp_round_id)
                        {
                            BP_DatabaseEntities.Match.Find(matches[j].matchId + match.Count - temp_match).takimID1
                                    = BP_DatabaseEntities.Match.Find(matches[j].matchId).takimID1;
                            BP_DatabaseEntities.SaveChanges();
                        }
                        if (matches[j + 1].takimID1 != null && matches[j + 1].takimID2 != null && matches[j + 1].skor1 != null && matches[j + 1].skor2 != null)
                        {
                            if (BP_DatabaseEntities.Match.Find(matches[j + 1].matchId).skor1 > BP_DatabaseEntities.Match.Find(matches[j + 1].matchId).skor2)
                            {
                                BP_DatabaseEntities.Match.Find(matches[j].matchId + match.Count - temp_match).takimID2
                                    = BP_DatabaseEntities.Match.Find(matches[j + 1].matchId).takimID1;
                            }
                            else
                            {
                                BP_DatabaseEntities.Match.Find(matches[j].matchId + match.Count - temp_match).takimID2
                                    = BP_DatabaseEntities.Match.Find(matches[j + 1].matchId).takimID2;
                            }
                            BP_DatabaseEntities.SaveChanges();
                        }
                        else if (matches[j + 1].takimID1 != null && matches[j + 1].takimID2 == null && r.roundId == temp_round_id)
                        {
                            BP_DatabaseEntities.Match.Find(matches[j].matchId + match.Count - temp_match).takimID2
                                    = BP_DatabaseEntities.Match.Find(matches[j + 1].matchId).takimID1;
                            BP_DatabaseEntities.SaveChanges();
                        }
                    }
                    k += 4;
                    temp_match++;
                }

            }
            ViewBag.turnuvaadi = t.turnuvaAdi;
            ViewBag.turnuvaid = id;
            ViewBag.turnuva_cesidi = t.turnuvaCesidi;
            return RedirectToAction("Yonet", "Home", new { id = id });
        }

        public static List<string> takimlar;
        [Authorize]
        [HttpPost]
        public ActionResult LigFiksture(List<string> elements, int groupNum)
        {
            Turnuva turnuva = new Turnuva();
            turnuva.kullaniciId = currentuser.kullaniciId;
            turnuva.turnuvaAdi = elements[0];
            turnuva.turnuvaCesidi = "lig";
            BP_DatabaseEntities.Turnuva.Add(turnuva);
            BP_DatabaseEntities.SaveChanges();

            List<Takim> takimlar_ = new List<Takim>();
            for (int i = 1; i < elements.Count; i++)
            {
                Takim takim = new Takim();
                takim.takimAdi = elements[i];
                takim.turnuvaId = turnuva.turnuvaId;
                BP_DatabaseEntities.Takim.Add(takim);
                BP_DatabaseEntities.SaveChanges();

            }
            BP_DatabaseEntities.Entry(turnuva).Collection(x => x.Takim).Load();
            ICollection<Takim> takimlar = turnuva.Takim;
            foreach (Takim t in takimlar)
                if (t.turnuvaId == turnuva.turnuvaId)
                    takimlar_.Add(t);
            List<Turnuva> turnuvalar = BP_DatabaseEntities.Turnuva.ToList();
            var turnuva_ = turnuvalar.FirstOrDefault(x => x.kullaniciId == currentuser.kullaniciId && x.turnuvaId == turnuva.turnuvaId);
            generateLigFiksture(takimlar_, turnuva_.turnuvaId, groupNum);
            BP_DatabaseEntities.Entry(turnuva_).Collection(x => x.Round).Load();
            ICollection<Round> rounds = turnuva_.Round;
            List<Match> matches = new List<Match>();
            foreach (Round r in rounds)
            {
                BP_DatabaseEntities.Entry(r).Collection(x => x.Match).Load();
                ICollection<Match> match = r.Match;
                foreach (Match m in match)
                    matches.Add(m);
            }
            ViewBag.turnuvaadi = turnuva_.turnuvaAdi;
            return RedirectToAction("Yonet", "Home", new { id = turnuva_.turnuvaId });
            //  return View(matches);
        }

        private void generateLigFiksture(List<Takim> takimList, int turnuvaId, int groupNum)
        {
            int groupParticipantNum = 0;
            if (groupNum == 0 || groupNum == 1)
            {
                createLigMatches(takimList, turnuvaId, 'A');
            }
            else
            {
                groupParticipantNum = takimList.Count / groupNum;
                int m = 0;
                List<Takim> groupParticipantList;
                int index = 0;

                char groupName = 'A';
                for (int i = 0; i < groupNum; i++)
                {
                    groupParticipantList = new List<Takim>();
                    for (int k = 0; k < groupParticipantNum; k++)
                    {
                        groupParticipantList.Add(takimList[index++]);
                        if (m < takimList.Count % groupNum && k == groupParticipantNum - 1)
                        {
                            groupParticipantList.Add(takimList[index++]);
                            m++;
                        }
                    }
                    createLigMatches(groupParticipantList, turnuvaId, groupName);
                    groupName++;
                }
            }
        }

        private void createLigMatches(List<Takim> grupList, int turnuvaId, char groupName)
        {
            if (grupList.Count == 3)
            {
                Round round = new Round();
                round.turnuvaId = turnuvaId;
                round.roundAdi = 1 + ". Round";
                BP_DatabaseEntities.Round.Add(round);
                BP_DatabaseEntities.SaveChanges();
                Match match = new Match();
                match.roundId = round.roundId;
                match.takimID1 = grupList[0].takimId;
                match.takimID2 = grupList[1].takimId;
                match.grupAdi = groupName.ToString();
                BP_DatabaseEntities.Match.Add(match);
                grupList[0].grupAdi = groupName.ToString();
                grupList[1].grupAdi = groupName.ToString();
                grupList[2].grupAdi = groupName.ToString();
                BP_DatabaseEntities.SaveChanges();

                Round round1 = new Round();
                round1.turnuvaId = turnuvaId;
                round1.roundAdi = 2 + ". Round";
                BP_DatabaseEntities.Round.Add(round1);
                BP_DatabaseEntities.SaveChanges();
                Match match1 = new Match();
                match1.roundId = round1.roundId;
                match1.takimID1 = grupList[0].takimId;
                match1.takimID2 = grupList[2].takimId;
                match1.grupAdi = groupName.ToString();
                BP_DatabaseEntities.Match.Add(match1);
                BP_DatabaseEntities.SaveChanges();

                Round round2 = new Round();
                round2.turnuvaId = turnuvaId;
                round2.roundAdi = 3 + ". Round";
                BP_DatabaseEntities.Round.Add(round2);
                BP_DatabaseEntities.SaveChanges();
                Match match2 = new Match();
                match2.roundId = round2.roundId;
                match2.takimID1 = grupList[1].takimId;
                match2.takimID2 = grupList[2].takimId;
                match2.grupAdi = groupName.ToString();
                BP_DatabaseEntities.Match.Add(match2);
                BP_DatabaseEntities.SaveChanges();

            }
            else
            {
                int roundCount = grupList.Count - 1;
                int matchCountPerRound = grupList.Count / 2;
                List<int> list = new List<int>();
                // Takim listesini oluşturuyoruz. 
                //0. takımdan (teamSize-1). takima kadar.
                for (int i = 0; i < grupList.Count; i++)
                {
                    list.Add(i);
                    grupList[i].grupAdi = groupName.ToString();
                    BP_DatabaseEntities.SaveChanges();
                }
                for (int i = 0; i < roundCount; i++)
                {
                    Round round = new Round();
                    round.turnuvaId = turnuvaId;
                    round.roundAdi = i + 1 + ". Round";
                    BP_DatabaseEntities.Round.Add(round);
                    BP_DatabaseEntities.SaveChanges();
                    List<Round> rounds = BP_DatabaseEntities.Round.ToList();
                    for (int j = 0; j < matchCountPerRound; j++)
                    {
                        int firstIndex = j;
                        int secondIndex = (grupList.Count - 1) - j;
                        Match match = new Match();
                        match.roundId = round.roundId;
                        match.takimID1 = grupList[list[firstIndex]].takimId;
                        match.takimID2 = grupList[list[secondIndex]].takimId;
                        match.grupAdi = groupName.ToString();
                        BP_DatabaseEntities.Match.Add(match);
                        BP_DatabaseEntities.SaveChanges();
                    }
                    // İlk eleman sabit olacak şekilde elamanları kaydırıyoruz
                    List<int> newList = new List<int>();
                    // İlk eleman sabit
                    newList.Add(list[0]);
                    // Son eleman ikinci eleman yapıyoruz.
                    newList.Add(list[list.Count - 1]);
                    for (int k = 1; k < list.Count - 1; k++)
                        newList.Add(list[k]);
                    // Keydırılan liste yeni liste oluyor.
                    list = newList;
                }
            }
        }

        private void generateEleme(List<Takim> grupList, int turnuvaId, int round)
        {
            int first_matches_count = 0;
            for (int i = 2; ; i *= 2)
            {
                if (grupList.Count <= i)
                {
                    Round _round = new Round();
                    _round.turnuvaId = turnuvaId;
                    _round.roundAdi = round + ". Round";
                    BP_DatabaseEntities.Round.Add(_round);
                    BP_DatabaseEntities.SaveChanges();
                    List<Round> rounds = BP_DatabaseEntities.Round.ToList();
                    var round_ = rounds.FirstOrDefault(x => x.roundAdi == round.ToString() + ". Round" && x.turnuvaId == turnuvaId);
                    for (int j = 0; j < i / 2; j++)
                    {
                        Match match = new Match();
                        match.roundId = round_.roundId;
                        match.takimID1 = grupList[j].takimId;
                        BP_DatabaseEntities.Match.Add(match);
                        BP_DatabaseEntities.SaveChanges();
                    }
                    int k = (i / 2);
                    List<Match> matches = BP_DatabaseEntities.Match.ToList();
                    foreach (Match m in matches)
                    {
                        if (m.roundId == round_.roundId)
                        {
                            m.takimID2 = grupList[k++].takimId;
                            BP_DatabaseEntities.SaveChanges();
                            if (grupList.Count == k)
                                break;
                        }
                    }
                    first_matches_count = i;
                    break;
                }
            }
            int round_num = round + 1;
            for (int j = first_matches_count / 2; 1 < j; j /= 2)
            {
                Round _round = new Round();
                _round.turnuvaId = turnuvaId;
                _round.roundAdi = round_num + ". Round";
                BP_DatabaseEntities.Round.Add(_round);
                BP_DatabaseEntities.SaveChanges();
                List<Round> rounds = BP_DatabaseEntities.Round.ToList();
                var round_ = rounds.FirstOrDefault(x => x.roundAdi == round_num.ToString() + ". Round" && x.turnuvaId == turnuvaId);
                for (int m = 0; m < j / 2; m++)
                {
                    Match match = new Match();
                    match.roundId = round_.roundId;
                    BP_DatabaseEntities.Match.Add(match);
                    BP_DatabaseEntities.SaveChanges();
                }
                round_num++;
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult ElemeKura(List<string> elements)
        {
            //turnuvayı veri tabanına ekler
            Turnuva turnuva = new Turnuva();
            turnuva.kullaniciId = currentuser.kullaniciId;
            turnuva.turnuvaAdi = elements[0];
            turnuva.turnuvaCesidi = "eleme";
            BP_DatabaseEntities.Turnuva.Add(turnuva);
            BP_DatabaseEntities.SaveChanges();

            //takımları veri tabanına ekler
            for (int i = 1; i < elements.Count; i++)
            {
                Takim grup = new Takim();
                grup.takimAdi = elements[i];
                grup.turnuvaId = turnuva.turnuvaId;
                BP_DatabaseEntities.Takim.Add(grup);
                BP_DatabaseEntities.SaveChanges();
            }

            //takimlari veri tabanından getirir ve eleme algoritmasını çalıştırır
            BP_DatabaseEntities.Entry(turnuva).Collection(x => x.Takim).Load();
            ICollection<Takim> takimlar = turnuva.Takim;
            List<Takim> takimlar_ = new List<Takim>();
            foreach (Takim t in takimlar)
                if (t.turnuvaId == turnuva.turnuvaId)
                    takimlar_.Add(t);
            List<Turnuva> turnuvalar = BP_DatabaseEntities.Turnuva.ToList();
            var turnuva_ = turnuvalar.FirstOrDefault(x => x.kullaniciId == currentuser.kullaniciId && x.turnuvaId == turnuva.turnuvaId);
            generateEleme(takimlar_, turnuva_.turnuvaId, 1);

            // round listesini veri tabanından getirip view'a gönderir
            BP_DatabaseEntities.Entry(turnuva_).Collection(x => x.Round).Load();
            ICollection<Round> rounds = turnuva_.Round;
            List<Round> round_list = new List<Round>();
            foreach (Round r in rounds)
                round_list.Add(r);
            ViewBag.turnuvaadi = turnuva_.turnuvaAdi;
            return RedirectToAction("Yonet", "Home", new { id = turnuva_.turnuvaId });
            //   return View(round_list);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ElemeyeGonder(int id, int takim_sayisi)
        {
            Turnuva turnuva = BP_DatabaseEntities.Turnuva.Find(id);
            BP_DatabaseEntities.Entry(turnuva).Collection(x => x.Takim).Load();
            ICollection<Takim> takimlar = turnuva.Takim;
            Turnuva yeni_turnuva = new Turnuva();
            yeni_turnuva.turnuvaAdi = turnuva.turnuvaAdi;
            yeni_turnuva.kullaniciId = currentuser.kullaniciId;
            yeni_turnuva.turnuvaCesidi = "lig-eleme";
            BP_DatabaseEntities.Turnuva.Add(yeni_turnuva);
            BP_DatabaseEntities.SaveChanges();
            List<Turnuva> turnuvaList = new List<Turnuva>();
            var groups = BP_DatabaseEntities.Turnuva.GroupBy(p => p.kullaniciId);
            foreach (var group in groups)
                if (group.Key.Value == currentuser.kullaniciId)
                    foreach (var t in group)
                        turnuvaList.Add(t);
            Turnuva son_turnuva = turnuvaList.LastOrDefault();
            var takimOrder = takimlar.OrderByDescending(x => x.pts).ThenByDescending(x => x.avg).GroupBy(x => x.grupAdi);
            List<Takim> gonderilecekTakimlar = new List<Takim>();
            foreach (var g in takimOrder)
            {
                List<Takim> order_takim_list = g.ToList<Takim>();
                int kacTakim = takim_sayisi;
                for (int i = 0; i < kacTakim; i++)
                    if (i < order_takim_list.Count)
                    {
                        Takim yeni_t = new Takim();
                        yeni_t.takimAdi = order_takim_list[i].takimAdi;
                        yeni_t.turnuvaId = son_turnuva.turnuvaId;
                        yeni_t.pts = order_takim_list[i].pts;
                        yeni_t.avg = order_takim_list[i].avg;
                        yeni_t.grupAdi = order_takim_list[i].grupAdi;
                        BP_DatabaseEntities.Takim.Add(yeni_t);
                        BP_DatabaseEntities.SaveChanges();
                        gonderilecekTakimlar.Add(order_takim_list[i]);

                    }
            }
            BP_DatabaseEntities.Entry(son_turnuva).Collection(x => x.Round).Load();
            ICollection<Round> rounds = son_turnuva.Round;
            generateEleme(gonderilecekTakimlar, son_turnuva.turnuvaId, 1);
            return RedirectToAction("Yonet", "Home", new { id = son_turnuva.turnuvaId });
        }

    }
}