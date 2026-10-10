# Estado del proyecto BERNEOX

## Situación actual

El documento maestro v2.0 es la referencia de producto. El proyecto Unity 6.6.5f1 está en `BERNEOXGame/`, creado con la plantilla Universal 3D. Se añadieron los primeros scripts de movimiento, joystick táctil y cámara de seguimiento para iniciar el prototipo Zombie Survival.

## Confirmado

- Aplicación Android con varios juegos.
- Zombie Survival será el primero.
- Unity 6 y C#.
- Firebase como plataforma de servicios; Google AdMob para anuncios.
- COINS son exclusivamente virtuales y no se canjean por dinero, tarjetas de regalo ni criptomonedas.
- El servidor tendrá autoridad sobre recompensas y operaciones críticas.
- Zombie Survival será 3D, horizontal y tendrá joystick en pantalla.

## Aún por decidir

- Perspectiva de cámara confirmada: tercera persona detrás del personaje. La altura, distancia y suavizado siguen provisionales hasta comprobarse dentro de Unity y en Android.
- Alcance exacto del primer prototipo jugable.
- Requisitos mínimos de Android y modo sin conexión.

## Estado del prototipo

Se crearon scripts para movimiento del personaje, cámara de seguimiento y joystick táctil, además de una herramienta de editor que genera una escena independiente `ZombiePrototype`. `SampleScene` se conserva. En Unity, la escena nueva se crea desde `BERNEOX > Crear escena de prototipo Zombie Survival`.

Todavía falta comprobar la escena dentro de Unity y hacer una compilación Android. No hay enemigos, combate ni interfaz final.

Las cifras de combate, recursos, fabricación y economía del documento están marcadas como provisionales; se ajustarán con pruebas.
