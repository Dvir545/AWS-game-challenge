import 'package:flame/game.dart';
import 'package:flame/components.dart';
import 'local/player/player.dart';

const String playerSpritesheet = 'Player/Player_Idle_Run_Death_Anim.png';
const double playerSpriteSize = 32;
const double playerHeightProportion = 0.15 / playerSpriteSize;

class MyGame extends FlameGame {
  late final SpriteAnimationComponent player;

  @override
  Future<void> onLoad() async {
    await super.onLoad();

    player = await loadPlayer(this);
  }
}
