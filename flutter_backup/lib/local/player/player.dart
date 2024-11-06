import 'package:flame/components.dart';
import 'package:flame/extensions.dart';
import 'package:flame/game.dart';
import '../../remote/load_images.dart';
import '../utils/image_manipulations.dart';


const String playerSpritesheet = 'Player/Player_Idle_Run_Death_Anim.png';
const double playerSpriteSize = 32;
const double playerHeightProportion = 0.15 / playerSpriteSize;

Future<Map<String, SpriteAnimation>> loadPlayerAnimations(Image spriteSheet) async{
    // add flip horizontally transformation
    final spriteSheetFlipped = await flipImage(spriteSheet);
    final mapping =  {
      'idleFront': SpriteAnimation.fromFrameData(
        spriteSheet,
        SpriteAnimationData.sequenced(
          amount: 6,
          stepTime: 0.1,
          textureSize: Vector2.all(playerSpriteSize),
        ),
      ),
      'idleRight': SpriteAnimation.fromFrameData(
        spriteSheet,
        SpriteAnimationData.sequenced(
          amount: 6,
          stepTime: 0.1,
          textureSize: Vector2.all(playerSpriteSize),
          texturePosition: Vector2(0, 1 * playerSpriteSize), // Second row
        ),
      ),
      'idleLeft': SpriteAnimation.fromFrameData(
        spriteSheetFlipped,
        SpriteAnimationData.sequenced(
          amount: 6,
          stepTime: 0.1,
          textureSize: Vector2.all(playerSpriteSize),
          texturePosition: Vector2(0, 1 * playerSpriteSize), // Second row
        ),
      ),
    };

    return mapping;

}

Future<SpriteAnimationComponent> loadPlayer(FlameGame game) async{
    final spriteSheet = await loadPlayerSpriteSheet(game);    
    final playerScale = game.size.y * playerHeightProportion;

    final animations = await loadPlayerAnimations(spriteSheet);

    SpriteAnimationComponent player = SpriteAnimationComponent(
      animation: animations['idleLeft']!,
      size: Vector2.all(playerSpriteSize),
      scale: Vector2.all(playerScale),
    );

    player.position = game.size / 2;
    player.anchor = Anchor.center;

    game.add(player);
    return player;
}