# BERNEOX

Aplicación Android de videojuegos en etapa inicial. El documento maestro está en `docs/BERNEOX final 2.docx`.

## Estado

El proyecto Unity 6.6.5f1 está en `BERNEOXGame/`, con plantilla Universal 3D y soporte Android instalado. Ya se añadieron los primeros scripts del prototipo Zombie Survival: movimiento, joystick táctil y cámara provisional. La escena base de Unity se conserva.

## Decisiones confirmadas

- Android como plataforma inicial; Unity 6 y C#.
- Zombie Survival será el primer juego, en 3D y horizontal, con joystick en pantalla.
- Firebase para servicios en la nube; AdMob para monetización.
- COINS será una moneda virtual interna, sin retiro ni conversión a dinero.
- Recompensas y otras operaciones críticas se validarán en el servidor.

## Siguiente paso

En Unity, crear la escena inicial desde el menú `BERNEOX > Crear escena de prototipo Zombie Survival` y pulsar Play. Detalles en `docs/PROTOTIPO_ZOMBIE.md`. El ángulo de cámara sigue pendiente y la cámara actual es provisional.
