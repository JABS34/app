# Product

<!-- impeccable:product-schema 1 -->

## Platform

android

## Users

Personas de 6 a 30 años que quieran jugar juegos móviles casuales y competitivos. La clasificación de edad, los controles parentales y los requisitos específicos para menores aún deben definirse antes de implementar cuentas, anuncios o compras.

## Product Purpose

BERNEOX reúne varios videojuegos en una sola aplicación Android. Los jugadores tendrán una cuenta y un perfil comunes para acceder a los juegos, seguir su progreso y sus récords, completar misiones y logros, y recibir recompensas internas. El éxito consiste en ofrecer una plataforma estable que pueda incorporar juegos progresivamente sin reconstruir la aplicación.

## Positioning

Una plataforma móvil que integra varios juegos en una aplicación con cuenta, perfil, progreso y COINS compartidos. La conexión en línea es deseada cuando sea posible; los requisitos de conexión y las funciones disponibles sin conexión siguen sin decidirse.

## Operating Context

- El producto comienza en Android y se desarrolla con Unity 6 y C#.
- Zombie Survival es el primer juego. Será 3D, se jugará en horizontal y tendrá joystick en pantalla.
- Los juegos previstos son Zombie Survival, Reflex, Memory, Trivia y Would You Rather. Cada juego posterior requiere especificación propia antes de implementarse.
- El documento maestro BERNEOX v2.0 está en `docs/BERNEOX final 2.docx`; distingue decisiones confirmadas, propuestas provisionales y asuntos pendientes.

## Capabilities and Constraints

- Firebase está seleccionado para autenticación, datos y servicios en la nube; Google AdMob para anuncios y se contemplan compras opcionales.
- COINS es moneda virtual interna, sin valor monetario real, retiro ni canje por dinero, tarjetas de regalo o criptomonedas.
- El servidor, no el cliente, será la autoridad para COINS, recompensas, resultados y compras.
- Los jugadores podrán tener progreso, récords, misiones y logros compartidos entre las áreas de la plataforma, según se implemente cada función.
- El objetivo de edades de 6 a 30 está confirmado. Clasificación de contenido, protección de menores, métodos de cuenta, políticas de anuncios y compras para menores permanecen pendientes.
- Se desea que la plataforma tenga funciones en línea, pero no se ha decidido si cada juego requerirá conexión ni qué funciones estarán disponibles sin conexión.
- La perspectiva de cámara de Zombie Survival está confirmada: tercera persona detrás del personaje. Su altura, distancia y suavizado, junto con el alcance exacto del primer prototipo, siguen provisionales o pendientes. Los valores de combate y economía del documento son provisionales hasta probarlos.

## Brand Commitments

- Nombre del producto: BERNEOX.
- La moneda se denomina COINS y nunca debe presentarse como dinero real ni como canjeable.
- El producto no debe prometer que es imposible de hackear; las medidas de seguridad reducen riesgos.

## Evidence on Hand

- `docs/BERNEOX final 2.docx`: documento maestro v2.0 con visión, juegos previstos, arquitectura y fases de desarrollo. Contiene datos provisionales y decisiones pendientes; su presencia no significa que esas funciones estén implementadas.
- El proyecto base de Unity está en `BERNEOXGame/`. El prototipo de Zombie Survival todavía no se ha validado en ejecución ni en un dispositivo Android.

## Product Principles

- Integrar juegos progresivamente en una sola plataforma.
- Priorizar estabilidad y rendimiento antes de ampliar la cantidad de contenido.
- Validar en el servidor las operaciones que afectan recompensas, resultados y compras.
- Tratar COINS únicamente como moneda virtual interna.
- Probar cada sistema y distinguir las propuestas de las decisiones aprobadas.
