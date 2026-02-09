#!/bin/bash

# Script de utilidad para tareas comunes de desarrollo

# Colores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# StrawberryShake incluye herramientas net9 que necesitan rollforward a net10
export DOTNET_ROLL_FORWARD=LatestMajor
APP_PROJECT="$SCRIPT_DIR/src/NexusMods.App/NexusMods.App.csproj"
REDENGINE_TESTS="$SCRIPT_DIR/tests/Games/NexusMods.Games.RedEngine.Tests"

echo -e "${BLUE}NexusMods.App - Herramientas de Desarrollo${NC}"
echo ""

show_menu() {
    echo "Selecciona una opcion:"
    echo ""
    echo "  1) Compilar solucion"
    echo "  2) Ejecutar app"
    echo "  3) Ejecutar todos los tests"
    echo "  4) Ejecutar tests (sin red/flakey)"
    echo "  5) Ejecutar tests de RedEngine (CP2077)"
    echo "  6) Ejecutar un test especifico"
    echo "  7) Limpiar proyecto"
    echo "  8) Restaurar dependencias"
    echo "  9) Todo (limpiar + restaurar + compilar + tests)"
    echo "  0) Salir"
    echo ""
    read -p "Opcion: " option
    echo ""
}

build_solution() {
    echo -e "${GREEN}Compilando solucion...${NC}"
    dotnet build "$SCRIPT_DIR"
}

run_app() {
    echo -e "${GREEN}Ejecutando app...${NC}"
    dotnet run --project "$APP_PROJECT"
}

run_all_tests() {
    echo -e "${GREEN}Ejecutando todos los tests...${NC}"
    dotnet test "$SCRIPT_DIR"
}

run_safe_tests() {
    echo -e "${GREEN}Ejecutando tests (sin red/flakey)...${NC}"
    dotnet test "$SCRIPT_DIR" --filter "RequiresNetworking!=True&FlakeyTest!=True"
}

run_redengine_tests() {
    echo -e "${GREEN}Ejecutando tests de RedEngine (CP2077)...${NC}"
    dotnet test "$REDENGINE_TESTS"
}

run_single_test() {
    read -p "Nombre del test (ej: SomeClass.SomeMethod): " test_name
    if [[ -z "$test_name" ]]; then
        echo -e "${YELLOW}No se especifico ningun test${NC}"
        return
    fi
    echo -e "${GREEN}Ejecutando test: $test_name${NC}"
    dotnet test "$SCRIPT_DIR" --filter "FullyQualifiedName~$test_name"
}

clean_project() {
    echo -e "${GREEN}Limpiando proyecto...${NC}"
    dotnet clean "$SCRIPT_DIR"
    echo -e "${GREEN}Proyecto limpio${NC}"
}

restore_deps() {
    echo -e "${GREEN}Restaurando dependencias...${NC}"
    dotnet restore "$SCRIPT_DIR"
}

run_all() {
    echo -e "${YELLOW}Ejecutando pipeline completo...${NC}"
    echo ""
    clean_project
    echo ""
    restore_deps
    echo ""
    build_solution
    if [[ $? -ne 0 ]]; then
        echo -e "${RED}La compilacion fallo. Abortando.${NC}"
        return
    fi
    echo ""
    run_safe_tests
    echo ""
    echo -e "${GREEN}Pipeline completado${NC}"
}

# Loop principal
while true; do
    show_menu

    case $option in
        1) build_solution ;;
        2) run_app ;;
        3) run_all_tests ;;
        4) run_safe_tests ;;
        5) run_redengine_tests ;;
        6) run_single_test ;;
        7) clean_project ;;
        8) restore_deps ;;
        9) run_all ;;
        0)
            echo -e "${BLUE}Hasta luego!${NC}"
            exit 0
            ;;
        *)
            echo -e "${YELLOW}Opcion invalida${NC}"
            ;;
    esac

    echo ""
    read -p "Presiona Enter para continuar..."
    clear
done
