using System.Threading.Tasks;
using Constants;
using Data;
using Managers;

namespace Castles
{
    public class OpponentCastleManager : CastleManager
    {
        protected override string _playerId { get; set; }
        protected override CastleData _castleData { get; set; }

        public override async Task Init(object[] args)
        {
            _playerId = Keys.OPPONENT_ID;
            
            await base.Init(args);
        }

        public void SetUpOpponent(CastleData castleData)
        {
            _castleData = castleData;
            
            // castle is shown when the game starts
            UpdateCastleWithCastleData();
        }
    }
}