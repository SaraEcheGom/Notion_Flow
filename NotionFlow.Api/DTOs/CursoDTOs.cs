namespace NotionFlow.Api.DTOs
{
    public record CrearCursoDto(
        string Nombre,
        string Materia,
        string ProfesorId
    );

    public record AsignarEstudianteDto(
        string EstudianteId
    );

    public record CursoResponseDto(
        int Id,
        string Nombre,
        string Materia,
        string ProfesorId,
        string ProfesorNombre,
        List<EstudianteDto> Estudiantes
    );

    public record EstudianteDto(
        string Id,
        string Nombre,
        string Email
    );

    public record CrearEvaluacionDto(
        string Titulo,
        string Descripcion,
        DateTime Fecha,
        double PorcentajeValor
    );

    public record GuardarNotaDto(
        string EstudianteId,
        double Valor
    );

    public record PublicarContenidoDto(
        string Titulo,
        string Descripcion,
        string Tipo,
        string Url
    );
}