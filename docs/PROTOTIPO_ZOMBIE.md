# Zombie Survival: primer prototipo

El prototipo incluye movimiento provisional en tercera persona detrás del personaje, movimiento relativo a la cámara, joystick táctil circular, vida, un botón contextual de ataque/recolección y un ciclo de hordas. Al comenzar, el jugador tiene 60 segundos de preparación (provisional); la primera horda tiene tres zombis de prueba. Tras derrotar la horda, hay 3 minutos hasta la siguiente. Cada ronda suma dos enemigos y aumenta provisionalmente su resistencia y velocidad. Los zombis derrotados dejan madera o chatarra; también hay materiales iniciales para recoger durante la preparación. Acércate y pulsa `RECOGER`, o usa `E` en computadora; el contador muestra totales locales. El botón vuelve a `ATACAR` cuando no hay recursos cerca. Los recursos no se guardan entre partidas y todavía no existe inventario completo, nodos de recolección, tala ni fabricación. Los tamaños de horda, espera inicial y dificultad son provisionales; el intervalo entre hordas está confirmado. Cámara y HUD aún deben validarse en Unity y Android. La herramienta de editor crea una escena nueva llamada `ZombiePrototype` y configura la orientación horizontal, conservando `SampleScene` y la escena de inicio BERNEOX.

Para crear la escena en Unity: espera a que termine de importar los scripts y usa el menú `BERNEOX > Crear escena de prototipo Zombie Survival`. En la escena, pulsa Play. En computadora se mueve con WASD/flechas, ataca con Espacio y recoge recursos con `E`; el joystick y el botón contextual en pantalla quedan preparados para táctil.

La perspectiva elegida es tercera persona detrás del personaje. La altura, distancia y suavizado de cámara siguen siendo valores provisionales. Los enemigos, el combate y la interfaz son prototipos funcionales sujetos a revisión visual; aún no se ha realizado una compilación Android.

## Modalidad de supervivencia acordada

Zombie Survival será una partida de supervivencia en un bosque nocturno organizada en rondas de hordas. El ciclo alterna preparación y peligro: explorar y recoger recursos del suelo, talar árboles, fabricar o mejorar equipo y preparar el refugio; después defenderse de la horda y aprovechar el descanso para recuperarse y prepararse para la siguiente.

- La primera ronda será fácil y servirá para aprender, reunir recursos y prepararse para una horda pequeña.
- Cada ronda posterior será gradualmente más difícil, elevando la presión sin volver la partida injusta. La progresión puede aumentar la cantidad o resistencia de los zombis y se calibrará jugando.
- La exploración y la recolección son actividades centrales, no solo matar zombis. Los materiales del concepto incluyen madera, piedra, fibra, chatarra y comida.
- Los zombis pueden dañar el refugio; construir, reforzar y reparar deben ser decisiones útiles para sobrevivir.
- El jugador confirmó 3 minutos desde el final de una horda hasta el inicio de la siguiente. La espera inicial sugerida por el documento maestro es de 60 segundos y sigue provisional.

El ciclo de rondas ya está implementado en el generador de escena, pero sigue siendo un prototipo sin tala, nodos de recursos, fabricación ni refugio. Estos sistemas se añadirán por etapas y no se consideran terminados.
