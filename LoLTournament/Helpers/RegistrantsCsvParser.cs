using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using MongoDB.Driver.Builders;
using Participant = LoLTournament.Models.Participant;

namespace LoLTournament.Helpers
{
    public static class RegistrantsCsvParser
    {
        public static void Check()
        {
            // Set all to false
            var allParticipants = Mongo.Participants.FindAll();

            foreach (var p in allParticipants)
            {
                p.RegisteredOfficially = false;
                Mongo.Participants.Save(p);
            }

            try
            {
                // Read csv
                var reader =
                    new StreamReader(File.OpenRead(HttpRuntime.AppDomainAppPath + "/Content/Admin/registrants.csv"));
                var stringWaiting = string.Empty;
                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) continue;

                    var split = line.Split(',');
                    long summonerId;
                    string registerStatus;

                    try
                    {
                        summonerId = long.Parse(split[3]);
                        registerStatus = split[0];
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    var participants =
                        Mongo.Participants.Find(Query<Participant>.Where(x => x.Summoner.Id == summonerId));

                    if (participants != null)
                    {
                        foreach (var p in participants)
                        {
                            p.RegisteredOfficially = registerStatus == "registered";
                            Mongo.Participants.Save(p);
                        }
                    }
                    else
                    {
                        // Put on waiting list
                        stringWaiting += line;
                    }
                }

                using (
                    var stream = File.Open(
                        HttpRuntime.AppDomainAppPath + "/Content/Admin/to_be_put_on_waitinglist.csv", FileMode.Create))
                {
                    var content = Encoding.UTF8.GetBytes(stringWaiting);
                    stream.Write(content, 0, content.Length);
                }

                var stringReminder =
                    Mongo.Participants.Find(Query<Participant>.Where(participant => !participant.RegisteredOfficially))
                        .Where(x => !x.Cancelled)
                        .Aggregate(string.Empty,
                            (current, participant) => current + participant.FullName + " <" + participant.Email + ">, ");

                using (
                    var stream = File.Open(
                        HttpRuntime.AppDomainAppPath + "/Content/Admin/not_yet_registered_email.csv", FileMode.Create))
                {
                    var content = Encoding.UTF8.GetBytes(stringReminder);
                    stream.Write(content, 0, content.Length);
                }

            }
            catch (Exception)
            {
                // Error
            }
        }
    }
}
