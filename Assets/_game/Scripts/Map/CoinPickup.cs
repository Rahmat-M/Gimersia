namespace Littale {
    public class CoinPickup : Pickup {
        PlayerCollector collector;
        public int coins = 1;

        protected override void OnDestroy() {
            base.OnDestroy();
            // Retrieve the CharacterCollector component from the player who picked up this object
            // Add coins to their total.
            if (target != null) {
                collector = target.GetComponentInChildren<PlayerCollector>();
                if (collector != null) collector.AddCoins(coins);

            }
        }
    }
}