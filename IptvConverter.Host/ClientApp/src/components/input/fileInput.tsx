import React, { useCallback, useRef, useState } from 'react';
import Button from '../button/button';

import './fileInput.scss';

interface PropsFile extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange'> {
	onChange(file: File): void;
}

const InputFile = ({ onChange, ...rest }: PropsFile) => {
	const [file, setFile] = useState<File | undefined>();
	const fileInputRef = useRef(null);

	const onChooseFileClick = useCallback(
		() => {
			(fileInputRef.current as any).click();
		},
		[],
	);

	const setFileCallback = useCallback(
		(e) => {
			const newFile = e.target.files[0];
			setFile(newFile);
			onChange(newFile);
		},
		[onChange],
	);

	return (
		<div className={'input-file-container'}>
			{!file &&
				<div className={'placeholder'}>No file selected</div>
			}
			{file &&
				<div className={'filename'}>{file.name}</div>
			}
            <label>
				<input
					{...rest}
					ref={fileInputRef}
					type='file'
                    onChange={setFileCallback}
				/>
				<Button
					onClick={onChooseFileClick}
				>Choose file</Button>
			</label>
		</div>
	);
};

export default InputFile;
