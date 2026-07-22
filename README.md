# Pokédex AR - Taller 3 PPA

Proyecto Unity 2022.3.62f3 para una Pokédex móvil en realidad aumentada. Incluye tres ImageTargets, ocho modelos 3D proporcionados por el taller, interacción táctil por raycast, navegación horizontal, panel informativo adaptable a 1080x2400, animación de presentación y sonido procedural.

## Contenido implementado

- ImageTarget `rati`: Rattata y Raticate.
- ImageTarget `greninja`: inicia en Greninja y permite recorrer Frogadier y Froakie.
- ImageTarget `abra`: Abra, Kadabra y Alakazam.
- Toque sobre el modelo 3D mediante raycast.
- Pokédex con nombre, tipo y descripción; cierre mediante botón `×`.
- Swipe horizontal para avanzar o retroceder entre evoluciones.
- Sonido distinto por etapa, generado completamente en C#.
- Movimiento de flotación y rotación suave.
- Diseño portrait con `CanvasScaler` a 1080x2400 y soporte de safe area.
- Los modelos aparecen únicamente mientras Vuforia mantiene el target en estado `TRACKED`.
- Ajuste de escala y orientación para mostrar cada Pokémon de pie sobre una carta vertical.
- Inicio explícito y diagnóstico visible de errores de cámara, licencia o compatibilidad.

## Abrir el proyecto

1. Abrir `taller3/` con Unity `2022.3.62f3`.
2. Abrir `Assets/Scenes/PokedexAR.unity`.
3. Pulsar Play para revisar la escena o compilar la APK para probar el seguimiento con la cámara del teléfono.

## Activar Vuforia 11.4.4

Vuforia Engine 11.4.4 está agregado como paquete local. La licencia está ligada a la cuenta del estudiante y se excluye de Git.

1. Abrir `Window > Vuforia Configuration`.
2. Pegar una licencia Development válida en `App License Key`.
3. Abrir `Assets/Scenes/PokedexAR.unity` y ejecutar `Pokedex AR > Preparar Vuforia`.
4. Compilar e instalar la APK en un teléfono Android compatible.

`VuforiaRuntimeTargetFactory` inicia la cámara y crea los observadores directamente desde las imágenes `rati`, `greninja` y `abra` exportadas con la base `taller_3`. Esto evita errores de carga del DAT en algunos dispositivos. La licencia privada permanece únicamente en `Assets/Resources/VuforiaConfiguration.asset` y nunca debe incorporarse a Git.

## ImageTargets imprimibles

- `taller3/Assets/Editor/Vuforia/ImageTargetTextures/taller_3/rati_scaled.jpg`
- `taller3/Assets/Editor/Vuforia/ImageTargetTextures/taller_3/greninja_scaled.jpg`
- `taller3/Assets/Editor/Vuforia/ImageTargetTextures/taller_3/abra_scaled.jpg`

Mostrar cada imagen completa en otra pantalla o imprimirla sin recortes. Para una detección estable, evitar reflejos, usar buena iluminación y mantener el teléfono aproximadamente a 20-40 cm.

## Compilar el APK

Con Vuforia y la licencia configurados:

1. Seleccionar `Pokedex AR > Build Android APK`.
2. El ejecutable se genera como `Builds/T3_21652040-8.apk`.

El proyecto ya está configurado en orientación vertical, IL2CPP, ARM64, Android API 26 como mínimo e identificador `cl.ucn.juanzuniga.pokedexar`.

## Estructura principal

- `Assets/Pokemon/Scripts`: mecánicas e integración AR documentadas.
- `Assets/Pokemon/Editor`: constructor reproducible de escena y APK.
- `Assets/Pokemon/Models`: modelos y texturas proporcionados para el taller.
- `Assets/StreamingAssets/Vuforia`: archivos DAT y XML de la base `taller_3`.
- `Assets/Editor/Vuforia/ImageTargetTextures/taller_3`: cartas usadas como ImageTargets.
- `Assets/Scenes/PokedexAR.unity`: escena principal.
- `Documentation`: informe, capturas y material de entrega.

## Autor

Juan Zúñiga Maluenda - RUT 21.652.040-8
