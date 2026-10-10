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

## Estado actual

La escena inicial de BERNEOX ya está configurada como primera escena. La pantalla principal y el catálogo son un prototipo runtime con apertura animada, tema claro/oscuro y objetos 3D. El Editor de Unity aún debe activarse para revisar la vista previa.

La perspectiva elegida para Zombie Survival es tercera persona detrás del personaje. Su escena incluye controles, zombis de prueba, vida y ataque cuerpo a cuerpo; cámara, valores y HUD todavía deben comprobarse. Consulta [docs/PROGRESS.md](docs/PROGRESS.md) para el estado detallado y [docs/PROTOTIPO_ZOMBIE.md](docs/PROTOTIPO_ZOMBIE.md) para cómo crear la escena.
