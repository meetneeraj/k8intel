using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace K8intel.Migrations
{
    /// <inheritdoc />
    public partial class phase3workingdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "text", nullable: false),
                    AgentApiKey = table.Column<string>(type: "text", nullable: false),
                    LastAgentContactAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterId = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClusterMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterId = table.Column<int>(type: "integer", nullable: false),
                    MetricType = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClusterMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClusterMetrics_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Fingerprint = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlertCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    KubeletVersion = table.Column<string>(type: "text", nullable: false),
                    OsImage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    TargetResource = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NodeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Namespace = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: false),
                    CpuRequest = table.Column<double>(type: "double precision", nullable: false),
                    MemoryRequest = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pods_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ClusterId_Timestamp_Filtered",
                table: "Alerts",
                columns: new[] { "ClusterId", "Timestamp", "Severity", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_ClusterMetrics_ClusterId_Type_Timestamp",
                table: "ClusterMetrics",
                columns: new[] { "ClusterId", "MetricType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_Name_Trgm",
                table: "Clusters",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gist")
                .Annotation("Npgsql:IndexOperators", new[] { "gist_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ClusterId_Fingerprint_Status",
                table: "Incidents",
                columns: new[] { "ClusterId", "Fingerprint", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ClusterId_Name",
                table: "Nodes",
                columns: new[] { "ClusterId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pods_NodeId_Name_Namespace",
                table: "Pods",
                columns: new[] { "NodeId", "Name", "Namespace" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_ClusterId_Type",
                table: "Recommendations",
                columns: new[] { "ClusterId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "ClusterMetrics");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "Pods");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "Clusters");
        }
    }
}
