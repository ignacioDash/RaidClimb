using System.Linq;
using System.Threading.Tasks;
using Constants;
using Data;
using Managers;
using Units;

namespace Castles
{
    public class PlayerCastleManager : CastleManager
    {
        protected override string _playerId { get; set; }
        protected override CastleData _castleData { get; set; }

        public override async Task Init(object[] args)
        {
            _playerId = Keys.PLAYER_ID;
            
            await base.Init(args);

            var dataManager = GameManager.Instance.GetManager<DataManager>();

            while (dataManager is not { Initialized: true })
            {
                await Task.Yield();
            }
            
            _castleData = dataManager.PlayerData.PlayerCastleData;

            // castle is shown at init
            UpdateCastleWithCastleData();
        }

        public override void OnGameStarted()
        {
            UpdateCastleWithCastleData();
        }
    }
}