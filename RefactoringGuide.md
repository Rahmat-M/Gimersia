# Modular Card System Refactoring Guide

We have refactored the Card System to be data-driven and modular. You no longer need to create a new C# script for every new card type. Instead, you can compose cards using **Actions**.

## Key Components

1.  **`CardActionSO`**: The base class for all card actions.
2.  **`SpawnPrefabActionSO`**: A concrete action that spawns a prefab (Projectile, Melee Hitbox, etc.).
3.  **`UniversalCardController`**: A generic controller that executes the list of actions defined in the Card Data.
4.  **`UniversalReactiveCardController`**: A generic controller for reactive cards.

## How to Create a New Card (The New Way)

### 1. Create an Action
First, define what the card does. For most attacks, you'll use `SpawnPrefabActionSO`.
1.  Right-click in Project view -> **Create -> Littale Data -> Card -> Actions -> Spawn Prefab**.
2.  Name it (e.g., `Action_SpawnFireball`).
3.  Configure settings:
    *   **Parent To Owner**: Check this for Melee attacks (sword swings). Uncheck for Projectiles.
    *   **Spawn Offset**: Adjust if needed.

### 2. Create/Update Card Data
1.  Select your `MainCardSO` (e.g., `Card_Fireball`).
2.  In the inspector, find the **Actions** list.
3.  Add your new Action (e.g., `Action_SpawnFireball`).
4.  Ensure `BehaviourPrefab` is assigned (this is what the action will spawn).

### 3. Setup the Card Prefab
1.  Open your Card Prefab.
2.  **Remove** the old specific script (e.g., `BasicProjectileCard`).
3.  **Add** the `UniversalCardController` component.
4.  Assign the `Card Data` field.

## Migrating Existing Cards

| Old Script | New Setup |
| :--- | :--- |
| `BasicMeleeCard` | Use `UniversalCardController` + `SpawnPrefabActionSO` (ParentToOwner = true) |
| `BasicProjectileCard` | Use `UniversalCardController` + `SpawnPrefabActionSO` (ParentToOwner = false) |
| `ReactiveCardController` | Use `UniversalReactiveCardController` + Add Actions to `ReactiveCardSO` |

## Extending the System

To add new behaviors (e.g., Heal, Buff), create a new script inheriting from `CardActionSO`:

```csharp
[CreateAssetMenu(menuName = "Littale Data/Card/Actions/Heal")]
public class HealActionSO : CardActionSO {
    public int healAmount;

    public override void PerformAction(BaseCard controller, BaseCardSO data) {
        // Logic to heal the player
        var stats = controller.GetComponentInParent<CharacterStats>();
        if (stats) stats.Heal(healAmount);
    }
}
```
