using System;
// using System.CodeDom;
// using System.CodeDom.Compiler;

using g4;

internal class Program {

    private static int Main(string[] args) {
        var unitCellLocation = "LatticeRobot-Diamond_TPMS";

        if (args.Length == 0)
            Console.WriteLine($"Using default unit cell: {unitCellLocation}.");
        else
            unitCellLocation = args[0];

        var codeRepPath = unitCellLocation;
        if (!Directory.Exists(codeRepPath)) {
            const string samplesRelPath = @"Samples";
            string repoDirPath = PathUtil.FindDirectoryContaining(samplesRelPath);

            // Find the code rep directory
            codeRepPath = Path.Combine(repoDirPath, samplesRelPath, unitCellLocation);
            PathUtil.AssertExists(codeRepPath);
        }

        var unitCell = new ImplicitUnitCell(codeRepPath, 2);

        // With this implementation of ImplicitUnitCell, we can only set constant parameters.  
        unitCell.SetParameter("gyroid", 0.25);

        Console.WriteLine("Computing mesh...");

        // generateMeshF() meshes the input implicit function at
        // the given cell resolution, and writes out the resulting mesh    
        Action<BoundedImplicitFunction3d, int, string> generateMeshF = (root, numcells, path) => {
            MarchingCubes c = new MarchingCubes();
            c.Implicit = root;
            c.RootMode = MarchingCubes.RootfindingModes.LerpSteps;      // cube-edge convergence method
            c.RootModeSteps = 5;                                        // number of iterations
            c.Bounds = root.Bounds();
            c.CubeSize = c.Bounds.MaxDim / numcells;
            c.Bounds.Expand(3 * c.CubeSize);                            // leave a buffer of cells
            c.Generate();
            MeshNormals.QuickCompute(c.Mesh);                           // generate normals
            StandardMeshWriter.WriteMesh(path, c.Mesh, WriteOptions.Defaults);   // write mesh
        };

        ImplicitSphere3d sphere = new() {
            Origin = Vector3d.Zero,
            Radius = 1.0
        };

        AxisAlignedBox3d bbox = new(-5 * Vector3d.One, 5 * Vector3d.One);

        ImplicitBox3d box = new ImplicitBox3d() {
            Box = new Box3d(bbox)
        };

        generateMeshF(new ImplicitIntersection3d() { A = unitCell, B = box }, 128, "LatticeRobot.obj");

        return 0;
    }

}
