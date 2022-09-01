use interoptopus::util::NamespaceMappings;
use interoptopus::{Error, Interop};

#[test]
#[cfg_attr(miri, ignore)]
fn bindings_csharp() -> Result<(), Error> {
    use interoptopus_backend_csharp::{Config, Generator};

    Generator::new(
        Config {
            class: "Rosu".to_string(),
            dll_name: "rosu_pp_ffi".to_string(),
            namespace_mappings: NamespaceMappings::new("RosuPP"),
            ..Config::default()
        },
        rosu_pp_ffi::my_inventory(),
    )
    .write_file("./bindings/bindings.cs")?;

    Ok(())
}

#[test]
#[cfg_attr(miri, ignore)]
fn bindings_c() -> Result<(), Error> {
    use interoptopus_backend_c::{Config, Generator};

    Generator::new(
        Config {
            ifndef: "rosu_pp".to_string(),
            ..Config::default()
        },
        rosu_pp_ffi::my_inventory(),
    )
    .write_file("bindings/bindings.h")?;

    Ok(())
}

#[test]
#[cfg_attr(miri, ignore)]
fn bindings_cpython_cffi() -> Result<(), Error> {
    use interoptopus_backend_cpython::{Config, Generator};

    let library = rosu_pp_ffi::my_inventory();
    Generator::new(Config::default(), library).write_file("bindings/bindings.py")?;

    Ok(())
}