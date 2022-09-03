use interoptopus::{Error, Interop};

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
