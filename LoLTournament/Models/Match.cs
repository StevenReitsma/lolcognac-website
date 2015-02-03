using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LoLTournament.Models
{
    // TODO wip
    public class Match
    {
        public ObjectId Id { get; set; }
        public ObjectId WinnerId { get; private set; }
        public TimeSpan Time { get; set; }
        public DateTime GameEndTime { get; set; }
        public List<ObjectId> ParticipantIds { get; private set; }
        public string GameMode { get; set; } // Should be CLASSIC
        public string GameType { get; set; } // Should be CUSTOM_GAME
        public bool Invalid { get; set; }
        public string SubType { get; set; } // Should be NORMAL
        public List<int> ChampionIds { get; set; }
        public Side WinnerSide { get; set; }

        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
    
        [BsonIgnore]
        public Team Winner
        {
            get
            {
                // Query team collection, return team with id WinnerId
                throw new NotImplementedException();
            }
        }

        [BsonIgnore]
        public List<Participant> Participants {
            get
            {
                // Query participants, return participants with id ParticipantIds
                throw new NotImplementedException();
            }
        } 
    }
}