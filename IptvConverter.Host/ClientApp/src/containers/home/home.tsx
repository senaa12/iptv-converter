import React, { useCallback, useState } from 'react';
import FileInput from '../../components/input/fileInput';
import Button from '../../components/button/button';

import './home.scss';

const home = () => {
    const [file, setFile] = useState<File | undefined>();
    const [loading, setLoading] = useState(false);

    const readPlaylistCallback = useCallback(
        () => {
            setLoading(true);

            setLoading(false);
        },
        [setLoading]
    )

    return (
        <div className={'home'}>
            <FileInput onChange={setFile} accept={'audio/x-mpegurl'}/>
            <Button 
                style={{ margin: 15 }}
                onClick={readPlaylistCallback}
            >READ PLAYLIST</Button>
        </div>
    )
}

export default home;