// here we load all images in the game
import 'package:flame/extensions.dart';
import 'package:flame/game.dart';

const String playerSpriteSheet = 'Player/Player_Idle_Run_Death_Anim.png';

Future<Image> loadPlayerSpriteSheet(FlameGame game) async{
  return await game.images.load(playerSpriteSheet);
}
