import React from "react";
import Spinner, { ClipSpinner } from '../spinner';

import './loading.scss';

const loading: React.FC = () => {
	
	return (
		<div className={'loading-wrapper'}>
			<Spinner text='Loading...' className={'loading-spinner'}>
				<ClipSpinner color='var(--white)' />
			</Spinner>
		</div>
	);
};

export default loading;