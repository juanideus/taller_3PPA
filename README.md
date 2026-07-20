# Pokédex AR - Taller 3 PPA

Proyecto Unity 2022.3.62f3 para una Pokédex móvil en realidad aumentada. Incluye dos líneas evolutivas completas, interacción táctil por raycast, navegación horizontal, panel informativo adaptable a 1080x2400, animación de presentación y sonido procedural.

## Contenido implementado

- ImageTarget psíquico: Abra, Kadabra y Alakazam.
- ImageTarget agua: Froakie, Frogadier y Greninja.
- Toque sobre el modelo 3D mediante raycast.
- Pokédex con nombre, tipo y descripción; cierre mediante botón `×`.
- Swipe horizontal para avanzar o retroceder entre evoluciones.
- Sonido distinto por etapa, generado completamente en C#.
- Movimiento de flotación y rotación suave.
- Diseño portrait con `CanvasScaler` a 1080x2400 y soporte de safe area.
- Escena de demostración compilable aunque Vuforia todavía no esté instalado.
- Integración automática con Instant Image Targets al detectar Vuforia 11.4.4.

## Abrir el proyecto

1. Abrir `taller3/` con Unity `2022.3.62f3`.
2. Abrir `Assets/Scenes/PokedexAR.unity`.
3. Pulsar Play para probar raycast, panel, botones y swipe con el mouse.

## Activar Vuforia 11.4.4

Vuforia requiere descarga y licencia ligadas a una cuenta del portal de PTC. El SDK y la clave no se versionan en Git.

1. Descargar Vuforia Engine `11.4.4` para Unity desde el portal oficial.
2. En Unity, abrir `Window > Package Manager`.
3. Usar `+ > Add package from tarball...` y seleccionar el archivo `.tgz` descargado.
4. Crear una licencia Development en Vuforia y pegarla en `Vuforia Configuration`.
5. Abrir `Assets/Scenes/PokedexAR.unity` y ejecutar la aplicación.

`VuforiaRuntimeTargetFactory` detecta el SDK por reflexión, agrega `VuforiaBehaviour` a la cámara y crea ambos ImageTargets desde las texturas del proyecto. No requiere generar una base de datos en Target Manager.

## ImageTargets imprimibles

- `taller3/Assets/Pokemon/Targets/AbraTarget.png`
- `taller3/Assets/Pokemon/Targets/FroakieTarget.png`

Imprimir cada imagen sin recortes, con un ancho aproximado de 12 cm. Las ilustraciones tienen detalle local, contraste y asimetría para favorecer el seguimiento.

## Compilar el APK

Con Vuforia y la licencia configurados:

1. Seleccionar `Pokedex AR > Build Android APK`.
2. El ejecutable se genera como `Builds/T3_21652040-8.apk`.

El proyecto ya está configurado en orientación vertical, IL2CPP, ARMv7/ARM64, Android API 26 como mínimo e identificador `cl.ucn.juanzuniga.pokedexar`.

## Estructura principal

- `Assets/Pokemon/Scripts`: mecánicas e integración AR documentadas.
- `Assets/Pokemon/Editor`: constructor reproducible de escena y APK.
- `Assets/Pokemon/Models`: modelos y texturas proporcionados para el taller.
- `Assets/Pokemon/Targets`: cartas usadas como ImageTargets.
- `Assets/Scenes/PokedexAR.unity`: escena principal.
- `Documentation`: informe, capturas y material de entrega.

## Autor

Juan Zúñiga Maluenda - RUT 21.652.040-8
