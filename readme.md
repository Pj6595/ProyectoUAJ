![flujo de trabajo de ejemplo](https://github.com/Pj6595/ProyectoUAJ/actions/workflows/BuildAndRunPipeline.yml/badge.svg)

# Proyecto UAJ - Tests de integración, flujo de Integración continua y alojamiento en servidor externo 

Este proyecto consiste en implementar tests de integración en el proyecto de MMOTFG. Para ello, hemos creado una serie de ficheros conteniendo una lista de comandos y, para cada uno, su salida esperada. Estas salidas se comparan con las del juego y, si no coinciden, se interpreta que hay un error y que no se debe llevar a cabo un commit.

Posteriormente, por medio de GitHub Actions, hemos configurado un pipeline de integración continua para que, cada vez que haya una solicitud de cambios en la rama principal del repositorio, se compruebe automáticamente que el motor compila correctamente y se pasen los tests de integración que se han configurado previamente. De esta forma se automatiza, en parte, el proceso de revisión de código por parte del resto de los miembros del equipo, sabiendo que en GitHub siempre habrá una versión funcional con los nuevos cambios.

Una vez que los cambios sean aprobados, se ejecutan una serie de acciones automáticas que crean una imagen de Docker con el nuevo código y la suben directamente a un registro de contenedores de Azure ya configurado previamente por nosotros. Este registro de contenedores está comunicado mediante un webhook a un servicio de aplicación de Azure, que automáticamente se actualiza con los datos de la nueva imagen, consiguiendo así que cada nueva versión del bot quede alojada automáticamente en un servidor externo.
