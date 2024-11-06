import 'dart:ui';

import 'package:flame/extensions.dart';

Future<Image> flipImage(Image image) async{
  final emptyPaint = Paint();
  final recorder = PictureRecorder();
  final canvas = Canvas(recorder);
  canvas.drawImage(image, Offset.zero, emptyPaint);
  canvas.scale(-1, 1);
  canvas.drawImage(image, Offset(-image.width * 2, 0), emptyPaint);

  final picture = recorder.endRecording();
  return await picture.toImageSafe(image.width * 2, image.height);
}